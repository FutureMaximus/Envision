using OpenTK.Mathematics;
using Envision.Graphics.Shaders.Data;

namespace Envision.Graphics.Lighting.Lights;

public class PBRPointLight : Light
{
    public PBRLightData LightData;

    public PBRPointLight(Vector3 position, PBRLightData lightData)
    {
        Position = position;
        LightData = lightData;
    }

    public override IShaderData GetShaderData() => LightData;
}
