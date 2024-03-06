using Envision.Core.Projects;
using Envision.Core.Projects.ProjectGroups;
using Envision.Graphics.Models;
using Envision.Graphics.Models.Generic;
using Envision.Graphics.Shaders;
using Envision.Graphics.Shaders.Data;
using Envision.Graphics.Textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Envision.Graphics.Render.RenderPasses;

public class ClusteredForwardRendering(Engine engine) : IRenderPass
{
    public Engine Engine { get; set; } = engine;

    /// <summary> Whether or not the render pass is enabled. </summary>
    public bool IsEnabled { get; set; }
    public ComputeShader? ClusterAABBShader { get; set; }
    public ComputeShader? ClusterLightCullShader { get; set; }
    public Shader? ClusterPBRShader { get; set; }

    public void Load()
    {
        if (Engine.ShaderHandler is null) return;
        // Get the AABBs of each cluster.
        ClusterAABBShader = new(Engine.ShaderHandler, "ClusterAABBShader", "clusterAABB");
        ClusterLightCullShader = new(Engine.ShaderHandler, "ClusterLightCullShader", "clusterCullLight");
        ClusterPBRShader = new(Engine.ShaderHandler, "ClusterPBRShader", "clusteredPBR");
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
        Project? activeProject = ProjectManager.ActiveProject;
        if (activeProject is null)
        {
            return;
        }

        if (Engine.ShaderHandler is null) return;
        ClusterLightCullShader?.Use();
        GL.DispatchCompute(1, 1, 6);
        GL.CullFace(CullFaceMode.Back);
        GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
        GL.StencilMask(0xFF);
        if (ClusterPBRShader is null)
        {
            throw new Exception("ClusterPBRShader is null.");
        }
        ClusterPBRShader.Use();
        ClusterPBRShader.SetFloat("zNear", Engine.EngineSettings.DepthNear);
        ClusterPBRShader.SetFloat("zFar", Engine.EngineSettings.DepthFar);

        List<GenericModel> genericModels = [];
        if (activeProject.SelectedProjectGroup is ModelProjectGroup modelProject)
        {
            if (modelProject.SelectedModel is GenericModel genericModel)
            {
                genericModels.Add(genericModel);
            }
        }

        foreach (GenericModel model in genericModels)
        {
            ModelRenderData modelRenderData = model.ModelRenderData;
            if (!modelRenderData.Visible) continue;

            foreach (GenericModelPart corePart in model.Parts)
            {
                Matrix4 meshTransform = corePart.Transformation;
                ClusterPBRShader.SetMatrix4("model", ref meshTransform);
                Matrix3 normalMat = corePart.NormalMatrix();
                ClusterPBRShader.SetMatrix3("normalMatrix", ref normalMat);
                foreach (GenericMesh mesh in corePart.Meshes)
                {
                    if (!mesh.IsLoaded)
                    {
                        mesh.Load();
                        continue;
                    }
                    ClusterPBRShader.SetBool("hasTangents", mesh.HasTangents);

                    PBRMaterialData pbrMat = mesh.ShaderData.MaterialData;
                    Texture2D albedo = pbrMat.AlbedoTexture;
                    Texture2D normal = pbrMat.NormalTexture;
                    Texture2D arm = pbrMat.ARMTexture;
                    if (!albedo.Loaded) continue;
                    albedo?.Use(TextureUnit.Texture1);
                    ClusterPBRShader.SetInt("material.albedoMap", 1);
                    if (!normal.Loaded) continue;
                    normal?.Use(TextureUnit.Texture2);
                    ClusterPBRShader.SetInt("material.normalMap", 2);
                    if (!arm.Loaded) continue;
                    arm?.Use(TextureUnit.Texture3);
                    ClusterPBRShader.SetInt("material.ARMMap", 3);

                    mesh.Render();
                }
            }
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
