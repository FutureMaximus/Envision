using System.Runtime.InteropServices;
using OpenTK.Mathematics;


namespace Envision.Graphics.Shaders.Data;

/// <summary>Light data relevant for culling.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 16)]
public struct GPUPointLightData(Vector3 pos, float maxRange)
{
    public readonly Vector3 Position => pos;
    public readonly float MaxRange => maxRange;
}
