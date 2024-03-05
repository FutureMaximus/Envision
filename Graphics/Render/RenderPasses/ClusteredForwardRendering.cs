using Envision.Graphics.Shaders;

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
            Shader.SetFloat(0, Engine.EngineSettings.DepthNear);
            Shader.SetFloat(1, Engine.EngineSettings.DepthFar);
        }
    }

    public void Render()
    {

    }

    public void Dispose()
    {
    }
}
