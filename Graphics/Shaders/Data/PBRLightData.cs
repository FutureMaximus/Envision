using OpenTK.Mathematics;

namespace Envision.Graphics.Shaders.Data;

public struct PBRLightData : IShaderData
{
    public readonly string Name => nameof(PBRLightData);

    /// <summary>The color of the light.</summary>
    public Vector3 Color;
    /// <summary>The intensity of the light this is multiplied with the attenuation factor.</summary>
    public float Intensity;
    /// <summary>The maximum range before this light is culled.</summary>
    public float MaxRange;
    /// <summary>Constant attenuation factor.</summary>
    public float Constant;
    /// <summary>Linear attenuation factor.</summary>
    public float Linear;
    /// <summary>Quadratic attenuation factor.</summary>
    public float Quadratic;

    public PBRLightData(Vector3 color, float intensity, float constant, float linear, float quadratic)
    {
        Color = color;
        Intensity = intensity;
        Constant = constant;
        Linear = linear;
        Quadratic = quadratic;
    }
    public PBRLightData(Vector3 color, float intensity)
    {
        Color = color;
        Intensity = intensity;
    }
}
