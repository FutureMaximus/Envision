using OpenTK.Mathematics;

namespace Envision.Graphics.Shaders.Data;

public struct PBRLightData : IShaderData
{
    public readonly string Name => nameof(PBRLightData);

    public Vector3 Color;
    public float Intensity;
    public float Constant;
    public float Linear;
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
