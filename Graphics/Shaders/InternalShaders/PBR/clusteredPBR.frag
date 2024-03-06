﻿#version 430 core

out vec4 FragColor;

in VS_OUT {
	vec3  FragPos;
	vec2  TexCoords;
	vec3  Normal;
	vec3  ViewPos;
	mat3  TBN;
	float zNear;
	float zFar;
} fs_in;

// ========== Uniforms ===============

struct Material
{
	sampler2D  albedoMap;
	sampler2D  normalMap;
	sampler2D  ARMMap; // R = Ambient Occlusion, G = Roughness, B = Metallic
};
uniform Material material;

uniform bool hasTangents;

struct DirectionalLight
{
	vec3  direction;
	vec3  color;
	float intensity;
};
uniform DirectionalLight dirLight;

// ===================================

// ====== Storage Buffer Objects ======

struct LightGrid
{
    uint offset;
    uint count;
};

struct PointLight
{
	vec3  position;
    float maxRange;
    vec3  color;
    float intensity;
    float constant;
    float linear;
    float quadratic;
};

layout (std430, binding = 2) buffer screenToView
{
    mat4 inverseProjection;
    uint tileSizeX;
    uint tileSizeY;
    uint tileSizeZ;
    vec2 tileSizePx;
    vec2 viewPxSize;
    float scale;
    float bias;
};

// The point light data.
layout (std430, binding = 3) buffer lightSSBO
{
    PointLight pointLights[];
};

layout (std430, binding = 4) buffer lightIndexSSBO
{
    uint globalLightIndexList[];
};

// The light grid this contains the offsets and counts for the lights within a grid.
layout (std430, binding = 5) buffer lightGridSSBO
{
    LightGrid lightGrid[];
};

// ====================================

const float PI = 3.14159265359;

// ========= PBR Methods =============
// D = Normal Distribution Function
// G = Geometric Shadowing Function
// F = Fresnel Reflectance
// V = View Vector
// L = Light Vector
// H = Halfway Vector
// N = Surface Normal
// R = Reflected Light Vector
// F0 = Reflectance at Normal Incidence
// F90 = Reflectance at grazing angle
// F = Fresnel Reflectance

// Microfacet distribution function that describes the statistical distribution of microfacets over a surface.
float D_GGX(vec3 N, vec3 H, float roughness)
{
	float a = roughness * roughness;
	float a2 = a * a;
	float NdotH = max(dot(N, H), 0.0);
	float NdotH2 = NdotH * NdotH;

	float numerator = a2;
	float denominator = (NdotH2 * (a2 - 1.0) + 1.0);
	denominator = PI * denominator * denominator;

	return numerator / max(denominator, 0.001);
}

// Schlick's approximation of the Fresnel reflectance as a function of the viewing angle.
float G_SchlickGGX(float NdotV, float roughness)
{
	float r = (roughness + 1.0);
	float k = (r * r) / 8.0;

	float numerator = NdotV;
	float denominator = NdotV * (1.0 - k) + k;

	return numerator / denominator;
}

// Geometric shadowing function that describes how much of the area of a microfacet is visible from a certain direction.
float G_Smith(vec3 N, vec3 V, vec3 L, float roughness)
{
	float NdotV = max(dot(N, V), 0.0);
	float NdotL = max(dot(N, L), 0.0);
	float ggx2 = G_SchlickGGX(NdotV, roughness);
	float ggx1 = G_SchlickGGX(NdotL, roughness);

	return ggx1 * ggx2;
}

// Used to determine how much the surface reflects light versus how much it refracts light.
vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}
// ===================================

// ======== Utility Methods ==========
// Returns the normal from the normal map this is used when tangents are not available.
vec3 GetNormalFromMap()
{
    vec3 tangentNormal = texture(material.normalMap, fs_in.TexCoords).xyz * 2.0 - 1.0;

    vec3 Q1  = dFdx(fs_in.FragPos);
    vec3 Q2  = dFdy(fs_in.FragPos);
    vec2 st1 = dFdx(fs_in.TexCoords);
    vec2 st2 = dFdy(fs_in.TexCoords);

    vec3 N   = normalize(fs_in.Normal);
    vec3 T   = normalize(Q1*st2.t - Q2*st1.t);
    vec3 B   = -normalize(cross(N, T));
    mat3 TBN = mat3(T, B, N);

    return normalize(TBN * tangentNormal);
}

// Linearize the depth value
float linearDepth(float depthSample)
{
    float ndc = 2.0 * depthSample - 1.0; // [-1, 1]
    return 2.0 * fs_in.zNear * fs_in.zFar / (fs_in.zFar + fs_in.zNear - ndc * (fs_in.zFar - fs_in.zNear)); // [0, 1]
}
// ===================================

vec3 CalcDirectionalLight(DirectionalLight light, vec3 N, vec3 V, vec3 fragPos, vec3 albedo, float rough, float metal, vec3 F0);
vec3 CalcPointLight(PointLight light, vec3 N, vec3 V, vec3 fragPos, vec3 albedo, float roughness, float metallic, vec3 F0);

void main()
{
    vec3 N;
	if (hasTangents)
	{
		N = texture(material.normalMap, fs_in.TexCoords).rgb;
		N = N * 2.0 - 1.0;   
		N = normalize(N * fs_in.TBN);
	}
	else // Fallback if tangents aren't available
	{
		N = GetNormalFromMap();
	}

	vec3 V = normalize(fs_in.ViewPos - fs_in.FragPos);

	vec3  albedo = pow(texture(material.albedoMap, fs_in.TexCoords).rgb, vec3(2.2));
	float ambientOcclusion = texture(material.ARMMap, fs_in.TexCoords).r;
	float roughness = texture(material.ARMMap, fs_in.TexCoords).g;
	float metallic  = texture(material.ARMMap, fs_in.TexCoords).b;

	// Calculate reflectance at normal incidence; if dia-electric (like plastic) use F0
	// of 0.04 and if it's a metal, use the albedo color as F0 (metallic workflow)
	vec3 F0 = vec3(0.04);
	F0 = mix(F0, albedo, metallic);

	// Locating which cluster you are a part of
    uint zTile     = uint(max(log2(linearDepth(gl_FragCoord.z)) * scale + bias, 0.0));
    uvec3 tiles    = uvec3( uvec2( gl_FragCoord.xy * tileSizePx ), zTile);
    uint tileIndex = tiles.x + tileSizeX * tiles.y + (tileSizeX * tileSizeY) * tiles.z;

	// Point lights
    uint lightCount       = lightGrid[tileIndex].count;
    uint lightIndexOffset = lightGrid[tileIndex].offset;

	// Reflectance equation
	vec3 Lo = vec3(0.0);
	Lo += CalcDirectionalLight(dirLight, N, V, fs_in.FragPos, albedo, roughness, metallic, F0);
	for (uint i = 0; i < lightCount; i++)
	{
		PointLight pointLight = pointLights[globalLightIndexList[lightIndexOffset + i]];
		Lo += CalcPointLight(pointLight, N, V, fs_in.FragPos, albedo, roughness, metallic, F0);
	}

	// Will be replaced by IBL
	vec3 ambient = vec3(0.03) * albedo * ambientOcclusion;

	vec3 color = ambient + Lo;

	// HDR
	color = color / (color + vec3(1.0));
	// Gamma correction
	color = pow(color, vec3(1.0 / 2.2));

	FragColor = vec4(color, 1.0);
}

vec3 CalcDirectionalLight(DirectionalLight light, vec3 N, vec3 V, vec3 fragPos, vec3 albedo, float rough, float metal, vec3 F0)
{
	// Calculate per-light radiance
	vec3 L = normalize(light.direction);
	vec3 H = normalize(V + L);
	vec3 radiance = light.color * light.intensity;

	float NDF = D_GGX(N, H, rough);
	float G = G_Smith(N, V, L, rough);
	vec3 F = fresnelSchlick(clamp(dot(H, V), 0.0, 1.0), F0);

	vec3  numerator = NDF * G * F;
	float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.001; // 0.001 to prevent divide by zero
	vec3  specular = numerator / denominator;

	vec3 kS = F;
	// For energy conservation, the diffuse and specular components should not exceed 1.0
	vec3 kD = vec3(1.0) - kS;
	// Ensures that only non-metals can have diffuse lighting or a linear blend if partly metal
	kD *= 1.0 - metal;

	float NdotL = max(dot(N, L), 0.0);

	return (kD * albedo / PI + specular) * radiance * NdotL;
}

vec3 CalcPointLight(PointLight light, vec3 N, vec3 V, vec3 fragPos, vec3 albedo, float rough, float metal, vec3 F0)
{
	vec3 L = normalize(light.position - fs_in.FragPos);
	vec3 H = normalize(V + L);
	float dist = length(light.position - fs_in.FragPos);
	// Inverse square law attenuation for control over the light's falloff.
	//float attenuation = 1.0 / (light.constant + light.linear * dist +
	//light.quadratic * (dist * dist));
	float attenuation = pow(clamp(1 - pow((dist / light.maxRange), 4.0), 0.0, 1.0), 2.0)/(1.0  + (dist * dist));
	// Intesity to control the brightness of the light
	//attenuation *= light.intensity;
	vec3  radiance = light.color * attenuation;

	float NDF = D_GGX(N, H, rough);
	float G = G_Smith(N, V, L, rough);
	vec3  F = fresnelSchlick(clamp(dot(H, V), 0.0, 1.0), F0);

	vec3  numerator = NDF * G * F;
	float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.001; // 0.001 to prevent divide by zero
	vec3  specular = numerator / denominator;

	vec3 kS = F;
	// For energy conservation, the diffuse and specular components should not exceed 1.0
	vec3 kD = vec3(1.0) - kS;
	// Ensures that only non-metals can have diffuse lighting or a linear blend if partly metal
	kD *= 1.0 - metal;

	float NdotL = max(dot(N, L), 0.0);

	return (kD * albedo / PI + specular) * radiance * NdotL;
}