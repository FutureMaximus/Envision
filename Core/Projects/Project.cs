using Envision.Core.Projects.ProjectGroups;
using Envision.Graphics.Models;
using Envision.Graphics.Models.Generic;
using Envision.Graphics.Models.ShapeModels;
using Envision.Graphics.Shaders.Data;
using Envision.Graphics.Textures;
using Envision.Util;
using System.Drawing;

namespace Envision.Core.Projects;

public sealed class Project : IDisposable
{
    public bool Active;
    public ProjectConfig Config = new("Project", new(0f, 0f, 0f));
    public bool Loaded;

    /// <summary> The project groups in the project. </summary>
    public readonly List<ProjectGroup> ProjectGroups = new();
    /// <summary> The project group that is currently selected. </summary>
    public ProjectGroup? SelectedProjectGroup;
    /// <summary> The objects that are currently selected in the project. </summary>
    public readonly List<IIdentifiable> OutlinedObjects = new();

    public Project()
    {
        Active = false;
    }

    public bool ContainsSceneObject(Guid guid) => ProjectGroups.Any(sceneObject => sceneObject.ID == guid);

    public static Project CreateDefaultProject()
    {
        Project defaultProject = new()
        {
            Active = true
        };

        GenericModel model = new("Sphere");
        GenericModelPart part = new("Sphere Part", model)
        {
            LocalTransformation = new()
            {
                Scale = new(1f)
            }
        };
        GenericModelPart part2 = new("Cube Part", model)
        {
            LocalTransformation = new()
            {
                Position = new(2f, 0f, 0f),
                Scale = new(1f)
            }
        };
        GenericModelPart part3 = new("Torus", model)
        {
            LocalTransformation = new()
            {
                Position = new(-2f, 0f, 2f),
                Scale = new(1f)
            }
        };
        Texture2D defaultAlbedo = TextureHelper.GenerateColorTexture(Color.Gray, 128, 128);
        Texture2D defaultNormal = TextureHelper.GenerateColorTexture(ColorHelper.DefaultNormalMapColor, 128, 128);
        Texture2D defaultARM = TextureHelper.GenerateColorTexture(Color.FromArgb(255, 127, 127), 128, 128);
        GenericMesh sphere = new Sphere(part, 1, 30, 30)
        {
            ShaderData = new GenericMeshShaderData(
                new PBRMaterialData()
                {
                    AlbedoTexture = defaultAlbedo,
                    NormalTexture = defaultNormal,
                    ARMTexture = defaultARM,
                }
            )
        };
        GenericMesh cube = new Cube(part2)
        {
            ShaderData = new GenericMeshShaderData(
                new PBRMaterialData()
                {
                    AlbedoTexture = defaultAlbedo,
                    NormalTexture = defaultNormal,
                    ARMTexture = defaultARM,
                }
            )
        };
        GenericMesh superFormula = new Torus(part3, 8.0f, 3.0f, 20, 20)
        {
            ShaderData = new GenericMeshShaderData(
                new PBRMaterialData()
                {
                    AlbedoTexture = defaultAlbedo,
                    NormalTexture = defaultNormal,
                    ARMTexture = defaultARM,
                }
            )
        };
        part.Meshes.Add(sphere);
        part2.Meshes.Add(cube);
        part3.Meshes.Add(superFormula);
        model.Parts.Add(part);
        model.Parts.Add(part2);
        model.Parts.Add(part3);
        
        List<IModel> modelList = new()
        {
            model
        };
        ModelProjectGroup modelGroup = new(modelList)
        {
            Name = "Models",
            SelectedModel = model
        };
        defaultProject.ProjectGroups.Add(modelGroup);
        defaultProject.SelectedProjectGroup = modelGroup;

        return defaultProject;
    }

    public void Load()
    {

    }

    public void Render()
    {

    }

    public void Dispose()
    {
    }
}
