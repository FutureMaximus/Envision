using Envision.Util;
using OpenTK.Graphics.OpenGL4;

namespace Envision.Graphics.Shaders;

/// <summary>Compute shader used for making parallel computations on the GPU.</summary>
public class ComputeShader : IShader
{
    public ShaderHandler ShaderHandler { get; }
    public string Name { get; }
    public int Handle { get; }

    public ComputeShader(ShaderHandler handler, string name, string sourceName)
    {
        if (handler.ShaderPath is null)
        {
            throw new NullReferenceException("Shader path is not set.");
        }
        ShaderHandler = handler;
        ShaderHandler.Shaders.Add(name, this);
        Name = name;
        Handle = GL.CreateShader(ShaderType.ComputeShader);
        string source = Shader.GetShaderFile(sourceName, "comp", handler);
        GL.ShaderSource(Handle, source);
        Shader.CompileShader(Handle, name);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Shader, Handle, name);
        GraphicsUtil.CheckError($"{name} Compute Shader");
    }

    public void Use() => GL.UseProgram(Handle);

    public void Dispose()
    {
        GL.DeleteShader(Handle);
        GC.SuppressFinalize(this);
    }
}
