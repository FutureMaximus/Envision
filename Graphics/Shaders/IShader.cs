namespace Envision.Graphics.Shaders;

public interface IShader : IDisposable
{
    ShaderHandler ShaderHandler { get; }
    string Name { get; }
    int Handle { get; }
}
