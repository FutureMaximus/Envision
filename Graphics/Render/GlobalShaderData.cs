using Envision.Graphics.Lighting;
using Envision.Graphics.Lighting.Lights;
using Envision.Graphics.Shaders.Data;
using Envision.Util;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Envision.Graphics.Render;

/// <summary>Handler for global shader data.</summary>
public static class GlobalShaderData
{
    /// <summary>Uniform buffer object that contains the projection, view, and view position.</summary>
    public static int ProjViewUBO { get; private set; }
    /// <summary>Shader storage buffer object that contains the cluster data.</summary>
    public static int ClusterSSBO { get; private set; }
    /// <summary>Shader storage buffer object that contains the screen to view data.</summary>
    public static int Screen2ViewSSBO { get; private set; }
    /// <summary>Shader storage buffer object that contains the light data.</summary>
    public static int LightDataSSBO { get; private set; }

    private static readonly uint GRID_SIZE_X = 16;
    private static readonly uint GRID_SIZE_Y = 9;
    private static readonly uint GRID_SIZE_Z = 4;
    private static readonly uint GRID_SIZE = GRID_SIZE_X * GRID_SIZE_Y * GRID_SIZE_Z;

    public static void LoadBuffers(Engine engine)
    {
        // Uniform Buffer Objects (Read-Only)
        ProjViewUBO = GL.GenBuffer();
        // 64 bytes for projection matrix, 64 bytes for view matrix, 16 bytes for camera position.
        GL.BindBuffer(BufferTarget.UniformBuffer, ProjViewUBO);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, ProjViewUBO, "ProjViewUBO Location 0");
        int projViewUBOSize = Marshal.SizeOf(typeof(Matrix4)) * 2 + Marshal.SizeOf(typeof(Vector3));
        GL.BufferData(BufferTarget.UniformBuffer, projViewUBOSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        GraphicsUtil.CheckError("UBO 0 (ProjView) Buffer Data");

        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, ProjViewUBO);
        GraphicsUtil.CheckError("UBO 0 (ProjView) Buffer Base");

        ProjViewUniform projViewUniform = new(engine.Projection, engine.Camera.View, engine.Camera.Position);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, Marshal.SizeOf(typeof(Matrix4)), ref projViewUniform.Projection);
        GL.BufferSubData(BufferTarget.UniformBuffer, Marshal.SizeOf(typeof(Matrix4)), Marshal.SizeOf(typeof(Matrix4)), ref projViewUniform.View);
        GL.BufferSubData(BufferTarget.UniformBuffer, Marshal.SizeOf(typeof(Matrix4)) * 2, Marshal.SizeOf(typeof(Vector3)), ref projViewUniform.ViewPos);
        GraphicsUtil.CheckError("UBO 0 (ProjView) Error");

        GL.BindBuffer(BufferTarget.UniformBuffer, 0);

        // Shader Storage Buffer Objects (Read-Write)
        // 4 bytes for min location (x, y, z) and 4 bytes for max location (x, y, z).
        ClusterSSBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ClusterSSBO);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, sizeof(float) * 8 * (int)GRID_SIZE, IntPtr.Zero, BufferUsageHint.StaticCopy);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, ClusterSSBO, "ClusterSSBO Location 1");
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, ClusterSSBO);
        GraphicsUtil.CheckError("SSBO 1 (Cluster) Buffer Base");
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

        Screen2View screen2View;
        Matrix4.Invert(engine.Projection, out screen2View.InverseProjection);
        screen2View.TileSizeX = GRID_SIZE_X;
        screen2View.TileSizeY = GRID_SIZE_Y;
        screen2View.TileSizeZ = GRID_SIZE_Z;
        if (engine.ScreenFBO is null)
        {
            throw new Exception("Screen FBO is null");
        }
        screen2View.TileSizePixels.X = 1f / MathF.Ceiling(engine.ScreenFBO.WindowSize.X / (float)GRID_SIZE_X);
        screen2View.TileSizePixels.Y = 1f / MathF.Ceiling(engine.ScreenFBO.WindowSize.Y / (float)GRID_SIZE_Y);
        screen2View.ViewPixelSize = new Vector2(1f / engine.ScreenFBO.WindowSize.X, 1f / engine.ScreenFBO.WindowSize.Y);
        // Basically reduced a log function into a simple multiplication an addition by pre-calculating these
        screen2View.SliceScalingFactor = GRID_SIZE_Z / MathF.Log2(engine.EngineSettings.DepthFar / engine.EngineSettings.DepthNear);
        screen2View.SliceBiasFactor = -(GRID_SIZE_Z * MathF.Log2(
            engine.EngineSettings.DepthNear) / MathF.Log2(engine.EngineSettings.DepthFar / engine.EngineSettings.DepthNear));
        Screen2ViewSSBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, Screen2ViewSSBO);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, Marshal.SizeOf(typeof(Screen2View)), IntPtr.Zero, BufferUsageHint.StaticCopy);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, Screen2ViewSSBO, "Screen2ViewSSBO Location 2");
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, Screen2ViewSSBO);
        GraphicsUtil.CheckError("SSBO 2 (Screen2View) Buffer Base");
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);

        LightDataSSBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, LightDataSSBO);
        GL.BufferData(BufferTarget.ShaderStorageBuffer, 
            Marshal.SizeOf(typeof(GPUPointLightData)) * engine.EngineSettings.MaximumLights, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, LightDataSSBO, "LightDataSSBO Location 3");
        int pointLightIndex = 0;
        for (int i = 0; i < engine.EngineSettings.MaximumLights; i++)
        {
            Light light = engine.Lights[i];
            if (light is PBRPointLight pointLight)
            {
                PBRLightData lightData = pointLight.LightData;
                GPUPointLightData gpuPointLightData = new(pointLight.Position, lightData.MaxRange);
                GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 
                    Marshal.SizeOf(typeof(GPUPointLightData)) * pointLightIndex,
                    Marshal.SizeOf(typeof(GPUPointLightData)), ref gpuPointLightData);
                GraphicsUtil.CheckError("SSBO 3 (LightData) Buffer Sub Data");
                pointLightIndex++;
            }
        }
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, LightDataSSBO);
        GraphicsUtil.CheckError("SSBO 3 (LightData) Buffer Base");
        GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
        
    }

    public static void UpdateProjViewUBO(ref ProjViewUniform projViewUniform)
    {
        GL.BindBuffer(BufferTarget.UniformBuffer, ProjViewUBO);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, Marshal.SizeOf(typeof(Matrix4)), ref projViewUniform.Projection);
        GL.BufferSubData(BufferTarget.UniformBuffer, Marshal.SizeOf(typeof(Matrix4)), Marshal.SizeOf(typeof(Matrix4)), ref projViewUniform.View);
        GL.BufferSubData(BufferTarget.UniformBuffer, Marshal.SizeOf(typeof(Matrix4)) * 2, Marshal.SizeOf(typeof(Vector3)), ref projViewUniform.ViewPos);
        GraphicsUtil.CheckError("UBO 0 (ProjView) Error");
    }

    public static void Dispose()
    {
        GL.DeleteBuffer(ProjViewUBO);
        GL.DeleteBuffer(ClusterSSBO);
        GL.DeleteBuffer(Screen2ViewSSBO);
        GL.DeleteBuffer(LightDataSSBO);
    }
}
