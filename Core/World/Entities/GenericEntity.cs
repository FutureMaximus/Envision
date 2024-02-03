using Envision.Graphics.Models.Generic;
using OpenTK.Mathematics;

namespace Envision.Core.World.Entities;

public class GenericEntity : WorldEntity
{
    public GenericModel GenericModel;
    public bool Instanced = false;

    public GenericEntity(GenericModel genericModel)
    {
        GenericModel = genericModel;
        _name = genericModel.Name;
    }

    public override string Name { get => _name; set => _name = value; }
    private string _name;

    public override Matrix4 Transformation()
    {
        if (WorldTransformation.HasChanged)
        {
            _transformation = WorldTransformation * GenericModel.Transformation;
        }
        return _transformation;
    }
    private Matrix4 _transformation = new();

    public override Matrix3 NormalMatrix()
    {
        Matrix4 transposeInv = Matrix4.Transpose(_transformation.Inverted());
        return new(
        transposeInv.M11, transposeInv.M12, transposeInv.M13,
        transposeInv.M21, transposeInv.M22, transposeInv.M23,
        transposeInv.M31, transposeInv.M32, transposeInv.M33);
    }
}
