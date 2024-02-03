using Envision.Graphics.Shaders.Data;
using Envision.Util;
using OpenTK.Mathematics;
using System.Drawing;

namespace Envision.Graphics.Lighting;

public abstract class Light : IIdentifiable
{
    public Vector3 Position;
    public Color Color;
    public float Intensity;
    public abstract IShaderData GetShaderData();
    public Guid ID => _id;
    private readonly Guid _id = Guid.NewGuid();
}
