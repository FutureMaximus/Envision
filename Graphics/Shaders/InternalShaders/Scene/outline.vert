#version 430 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;

layout (std140, binding = 0) uniform ProjView
{
	mat4 projection;
	mat4 view;
	vec3 viewPos;
};

uniform mat4 model;
uniform mat3 normalMatrix;

void main()
{
	vec3 normal = normalize(normalMatrix * aNormal);
	gl_Position = vec4(aPosition + normal * 0.03f, 1.0f) * model * view * projection;
}