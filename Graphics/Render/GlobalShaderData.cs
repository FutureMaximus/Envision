using Envision.Graphics.Shaders.Data;
using Envision.Util;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Envision.Graphics.Render;

/// <summary>Handler for global shader data.</summary>
public static class GlobalShaderData
{
    public static void Initialize(Engine engine)
    {
        // Uniform Buffer Objects (Read-Only)
        engine.ProjViewUBO = GL.GenBuffer();
        // 64 bytes for projection matrix, 64 bytes for view matrix, 16 bytes for camera position.
        GL.BindBuffer(BufferTarget.UniformBuffer, engine.ProjViewUBO);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, engine.ProjViewUBO, "ProjViewUBO Location 0");
        int projViewUBOSize = Marshal.SizeOf(typeof(Matrix4)) * 2 + Marshal.SizeOf(typeof(Vector3));
        GL.BufferData(BufferTarget.UniformBuffer, projViewUBOSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        GraphicsUtil.CheckError("UBO 0 (ProjView) Buffer Data");

        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, engine.ProjViewUBO);
        GraphicsUtil.CheckError("UBO 0 (ProjView) Buffer Base");

        ProjViewUniform projViewUniform = new(engine.Projection, engine.Camera.View, engine.Camera.Position);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, Marshal.SizeOf(typeof(Matrix4)), ref projViewUniform.Projection);
        GL.BufferSubData(BufferTarget.UniformBuffer, Marshal.SizeOf(typeof(Matrix4)), Marshal.SizeOf(typeof(Matrix4)), ref projViewUniform.View);
        GL.BufferSubData(BufferTarget.UniformBuffer, Marshal.SizeOf(typeof(Matrix4)) * 2, Marshal.SizeOf(typeof(Vector3)), ref projViewUniform.ViewPos);
        GraphicsUtil.CheckError("UBO 0 (ProjView) Error");

        GL.BindBuffer(BufferTarget.UniformBuffer, 0);

        // Shader Storage Buffer Objects (Read-Write)
    }
}
