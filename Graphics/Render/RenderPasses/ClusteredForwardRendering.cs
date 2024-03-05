using Envision.Graphics.Shaders;
using OpenTK.Graphics.OpenGL4;

namespace Envision.Graphics.Render.RenderPasses;

public class ClusteredForwardRendering(Engine engine) : IRenderPass
{
    public Engine Engine { get; set; } = engine;

    /// <summary> Whether or not the render pass is enabled. </summary>
    public bool IsEnabled { get; set; }
    public ComputeShader? ClusterAABBShader { get; set; }

    public void Load()
    {
        if (Engine.ShaderHandler is null) return;
        ClusterAABBShader = new("ClusterAABBShader", "clusterAABB", Engine.ShaderHandler);
        if (ClusterAABBShader is not null)
        {
            ClusterAABBShader.Use();
            Shader.SetFloat(0, Engine.EngineSettings.DepthNear); // zNear
            Shader.SetFloat(1, Engine.EngineSettings.DepthFar); // zFar
            GL.DispatchCompute(GlobalShaderData.GRID_SIZE_X, GlobalShaderData.GRID_SIZE_Y, GlobalShaderData.GRID_SIZE_Z);
        }
    }

    public void Render()
    {

    }

    public void Dispose()
    {
    }
}
