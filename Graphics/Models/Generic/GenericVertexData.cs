using OpenTK.Mathematics;

namespace Envision.Graphics.Models.Generic;

public struct GenericVertexData
{
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 TextureCoords;

    public GenericVertexData(Vector3 position, Vector3 normal, Vector2 texCoords)
    {
        Position = position;
        Normal = normal;
        TextureCoords = texCoords;
    }
}
