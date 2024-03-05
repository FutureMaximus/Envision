using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Envision.Graphics.Shaders;

/// <summary>Compute shader used for making parallel computations on the GPU.</summary>
public class ComputeShader : IShader
{
    public ShaderHandler ShaderHandler { get; }
    public string Name { get; }
    public int Handle { get; }

    public ComputeShader(string name, string sourceName, ShaderHandler handler)
    {
        if (handler.ShaderPath is null)
        {
            throw new NullReferenceException("Shader path is not set.");
        }
        ShaderHandler = handler;
        Name = name;
        Handle = GL.CreateShader(ShaderType.ComputeShader);
        string source = Shader.GetShaderFile(sourceName, "comp", handler);
        GL.ShaderSource(Handle, source);
        Shader.CompileShader(Handle, name);
    }

    public void Dispose()
    {
        GL.DeleteShader(Handle);
        GC.SuppressFinalize(this);
    }
}
