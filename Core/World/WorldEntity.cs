using Envision.Graphics.Models;
using OpenTK.Mathematics;

namespace Envision.Core.World;

public abstract class WorldEntity
{
    public virtual string Name { get; set; } = "Entity";
    public Transformation WorldTransformation = new();
    public abstract Matrix4 Transformation();
    public abstract Matrix3 NormalMatrix();
}
