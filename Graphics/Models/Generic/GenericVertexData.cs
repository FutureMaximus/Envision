using Envision.Util;
using OpenTK.Mathematics;

namespace Envision.Graphics.Models.Generic;

public struct GenericVertexData(Vector3 position, Vector3 normal, Vector2 texCoords)
{
    public Vector3 Position = position;
    public Vector3 Normal = normal;
    public Vector2 TextureCoords = texCoords;
    public int[] BoneIDs = GraphicsUtil.EmptyBoneIDs();
    public float[] Weights = GraphicsUtil.EmptyBoneWeights();
}
