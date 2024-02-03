using OpenTK.Mathematics;
using Envision.Graphics.Shaders.Data;
using Envision.Util;

namespace Envision.Graphics.Lighting.Lights;

/// <summary> The direction of this light is the position normalized. </summary>
public class PBRDirectionalLight : Light
{
    public PBRLightData LightData;

    public PBRDirectionalLight(Vector3 direction, PBRLightData lightData)
    {
        Position = direction.Normalized();
        LightData = lightData;
    }

    public override IShaderData GetShaderData() => LightData;
}
