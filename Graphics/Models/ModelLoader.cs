﻿using Assimp;
using OpenTK.Mathematics;
using Envision.Graphics.Shaders.Data;
using Envision.Graphics.Textures;
using Envision.Util;
using Envision.Graphics.Models.Generic;

namespace Envision.Graphics.Models;

/// <summary>
/// Processes PBR models and stores them in a list.
/// When you get a model from the entry it is removed from the list.
/// 
/// <para>
/// For arm textures the file name must contain arm, rough, metal, or ao in it otherwise it will not be loaded.
/// If there is no arm texture then roughness, metallic and ambient occlusion textures will be loaded instead.
/// </para>
/// </summary>
public static class ModelLoader
{
    private static readonly List<GenericModel> _processedModels = [];
    private static readonly List<LoadedTexture> _processedTextures = [];

    public struct ModelEntry
    {
        public string Name;
        public string ModelFile;
        public string Path;
        public string TexturePath;
        public GenericModel? CoreModel;

        public ModelEntry(string name, string modelFile, string path, string texturePath)
        {
            Name = name;
            ModelFile = modelFile;
            Path = path;
            TexturePath = texturePath;
        }
    }

    public struct LoadedTexture
    {
        public string ModelInternalName;
        public string Name;
        public string Path;

        public LoadedTexture(string modelName, string name, string path)
        {
            ModelInternalName = modelName;
            Name = name;
            Path = path;
        }

        public override readonly bool Equals(object? obj)
        {
            if (obj is not LoadedTexture texture)
            {
                return false;
            }
            return texture.Path == Path;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(ModelInternalName, Name, Path);
        }

        public static bool operator ==(LoadedTexture left, LoadedTexture right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LoadedTexture left, LoadedTexture right)
        {
            return !(left == right);
        }
    }

    /// <summary> Data for a node in the model. </summary>
    public struct AssimpNodeData
    {
        public string Name;
        public Matrix4 Transformation;
        public int ChildrenCount;
        public List<AssimpNodeData> Children;
    }

    /// <summary>
    /// Loads a model from the given path. You can specify the post processing steps as well.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="directory"></param>
    /// <param name="postProcessSteps"></param>
    /// <param name="assetStreamer"></param>
    /// <exception cref="AssimpException"></exception>
    public static void ProcessModel(ModelEntry modelEntry, AssetStreamer assetStreamer, PostProcessSteps postProcessSteps = 
        PostProcessSteps.Triangulate | 
        PostProcessSteps.SortByPrimitiveType |
        PostProcessSteps.FlipUVs |
        PostProcessSteps.CalculateTangentSpace
    )
    {
        AssimpContext importer = new();
        Scene scene;
        try
        {
            scene = importer.ImportFile(Path.Combine(modelEntry.Path, modelEntry.ModelFile), postProcessSteps);
            if (scene == null)
            {
                throw new AssimpException($"Could not get scene while loading model {modelEntry.Name}");
            }
        }
        catch (AssimpException e)
        {
            throw new AssimpException($"Failed to load model {modelEntry.Name}: {e}");
        }
        string internalName = modelEntry.Path.Split('\\').Last().Split('/').Last();
        GenericModel model = new(internalName);
        modelEntry.CoreModel = model;
        _processedModels.Add(model);
        ProcessNode(scene.RootNode, scene, modelEntry, assetStreamer);
    }

    /// <summary>
    /// After getting a model from the entry, it is removed from the processed models list.
    /// </summary>
    /// <param name="name"></param>
    /// <returns>
    /// The model if it exists, otherwise null.
    /// </returns>
    public static GenericModel? GetModel(string name)
    {
        for (int i = 0; i < _processedModels.Count; i++)
        {
            if (_processedModels[i].Name == name)
            {
                GenericModel model = _processedModels[i];
                _processedModels.RemoveAt(i);
                foreach (LoadedTexture loadedTex in _processedTextures.ToList())
                {
                    if (loadedTex.ModelInternalName == model.Name)
                    {
                        _processedTextures.Remove(loadedTex);
                    }
                }
                return model;
            }
        }
        return null;
    }

    private static void ProcessNode(Node Node, Scene scene, ModelEntry modelEntry, AssetStreamer assetStreamer, GenericModelPart? parent = null)
    {
        if (modelEntry.CoreModel is null) return;
        GenericModelPart modelPart = new(Node.Name, modelEntry.CoreModel);
        if (parent != null)
        {
            modelPart.Parent = parent;
        }

        Matrix4x4 transform = Node.Transform;
        Matrix4 modelTransform = new(
                  transform.A1, transform.A2, transform.A3, transform.A4,
                  transform.B1, transform.B2, transform.B3, transform.B4,
                  transform.C1, transform.C2, transform.C3, transform.C4,
                  transform.D1, transform.D2, transform.D3, transform.D4);
        modelTransform.Transpose();
        modelPart.LocalTransformation = new(modelTransform);

        modelEntry.CoreModel.Parts.Add(modelPart);

        for (int i = 0; i < Node.MeshCount; i++)
        {
            Assimp.Mesh mesh = scene.Meshes[Node.MeshIndices[i]];
            GenericMesh? modelMesh = ProcessMesh(mesh, scene, modelEntry, assetStreamer, modelPart);
            if (modelMesh is null)
            {
                continue;
            }
            modelPart.Meshes.Add(modelMesh);
        }

        for (int i = 0; i < Node.ChildCount; i++)
        {
            ProcessNode(Node.Children[i], scene, modelEntry, assetStreamer, modelPart);
        }
    }

    private static GenericMesh? ProcessMesh(Assimp.Mesh mesh, Scene scene, ModelEntry modelEntry, AssetStreamer assetStreamer, GenericModelPart modelPart)
    {
        List<Vector3> positions = [];
        List<Vector3> normals = [];
        List<Vector2> textureCoords = [];
        List<Vector3> tangents = [];
        List<uint> indices = [];

        for (int i = 0; i < mesh.VertexCount; i++)
        {
            if (!mesh.HasNormals)
            {
                throw new AssimpException($"Mesh {mesh.Name} in model {modelEntry.Name} does not have normals.");
            }
            if (!mesh.HasTextureCoords(0))
            {
                textureCoords.Add(Vector2.Zero);
            }
            else
            {
                textureCoords.Add(new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y));
            }

            positions.Add(new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z));
            normals.Add(new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z));
            tangents.Add(new Vector3(mesh.Tangents[i].X, mesh.Tangents[i].Y, mesh.Tangents[i].Z));
        }

        for (int i = 0; i < mesh.FaceCount; i++)
        {
            Face face = mesh.Faces[i];
            for (int j = 0; j < face.IndexCount; j++)
            {
                indices.Add((uint)face.Indices[j]);
            }
        }

        if (mesh.MaterialIndex < 0)
        {
            DebugLogger.Log($"Mesh {mesh.Name} in model {modelEntry.Name} does not have a material.");
            return null;
        }
        Material material = scene.Materials[mesh.MaterialIndex];

        // 1. Albedo maps
        List<Texture2D> albedoMaps = LoadMaterialTextures(material, TextureType.Diffuse, modelEntry, assetStreamer, mesh);

        // 2. Normal maps
        List<Texture2D> normalMaps = LoadMaterialTextures(material, TextureType.Normals, modelEntry, assetStreamer, mesh);

        // 3. ARM maps (Ambient Occlusion, Roughness, Metallic)
        List<Texture2D> armMaps = LoadMaterialTextures(material, TextureType.Unknown, modelEntry, assetStreamer, mesh);

        // 4. Height maps (Optional)
        List<Texture2D> heightMaps = LoadMaterialTextures(material, TextureType.Height, modelEntry, assetStreamer, mesh);

        Texture2D albedoTexture;
        try
        {
            albedoTexture = albedoMaps[0];
        } catch (ArgumentOutOfRangeException)
        {
            throw new AssimpException($"Mesh {mesh.Name} in model {modelEntry.Name} does not have an albedo texture.");
        }

        Texture2D normalTexture;
        try
        {
            normalTexture = normalMaps[0];
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new AssimpException($"Mesh {mesh.Name} in model {modelEntry.Name} does not have a normal texture.");
        }

        Texture2D armTexture;
        try
        {
            armTexture = armMaps[0];
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new AssimpException($"Mesh {mesh.Name} in model {modelEntry.Name} does not have an ARM texture.");
        }

        Texture2D? heightMap = heightMaps.Count > 0 ? heightMaps[0] : null;

        PBRMaterialData meshMaterial = new(albedoTexture, normalTexture, armTexture);
        if (heightMap is not null)
        {
            meshMaterial.HeightTexture = heightMap;
        }
        GenericMeshShaderData meshShaderData = new(meshMaterial); 

        GenericMesh modelMesh = new(mesh.Name, modelPart)
        {
            ShaderData = meshShaderData,
            Vertices = positions,
            TextureCoords = textureCoords,
            Normals = normals,
            Tangents = tangents,
            Indices = indices
        };

        return modelMesh;
    }

    private static List<Texture2D> LoadMaterialTextures(Material mat, TextureType type, ModelEntry modelEntry, AssetStreamer assetStreamer, Assimp.Mesh mesh)
    {
        List<Texture2D> loadedTextures = [];
        for (int i = 0; i < mat.GetMaterialTextureCount(type); i++)
        {
            mat.GetMaterialTexture(type, i, out TextureSlot tex);
            string filePath = tex.FilePath;
            if (filePath == string.Empty || filePath == null) continue;

            if (type == TextureType.Unknown
                && !filePath.Contains("arm") 
                && !filePath.Contains("rough") 
                && !filePath.Contains("metal")
                && !filePath.Contains("ao"))
            {
                throw new AssimpException($"Mesh {mesh.Name} in model {modelEntry.Name} does not have an ARM texture.");
            }

            string internalModelName = modelEntry.CoreModel?.Name ?? throw new Exception("Model entry core model is null.");
            string texName = $"{internalModelName}_{Path.GetFileNameWithoutExtension(filePath)}";
            LoadedTexture loadedTexture = new(internalModelName, texName, tex.FilePath);

            if (_processedTextures.Contains(loadedTexture))
            {
                TextureEntries.GetTexture(loadedTexture.Name, out Texture2D? texture);
                loadedTextures.Add(texture);
                break;
            }
            _processedTextures.Add(loadedTexture);

            string texturePath = Path.Combine(modelEntry.Path, filePath);
            Texture2D materialTexture = new(texName);
            TextureEntries.AddTexture(materialTexture);
            TextureHelper.LoadTextureFromAssetStreamer(materialTexture, texturePath, assetStreamer);
            loadedTextures.Add(materialTexture);
        }
        return loadedTextures;
    }
}
