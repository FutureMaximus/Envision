using Envision.Graphics.Shaders;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Envision.Graphics.Render.RenderPasses;

public class ClusteredForwardRendering : IRenderPass
{
    public Engine Engine { get; set; }

    /// <summary> Whether or not the render pass is enabled. </summary>
    public bool IsEnabled { get; set; }
    public ComputeShader? ClusterAABBShader { get; set; }

    public ClusteredForwardRendering(Engine engine)
    {
        Engine = engine;
        ClusterAABBShader = engine.ShaderHandler?.GetShader("ClusterAABB") as ComputeShader;
    }

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
        throw new NotImplementedException();
    }

    public void Dispose()
    {
    }
}
