using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Envision.Graphics.Lighting;
using Envision.Graphics.Lighting.Lights;
using Envision.Graphics.Models;
using Envision.Graphics.Shaders;
using Envision.Graphics.Shaders.Data;
using Envision.Graphics.Textures;
using Envision.Graphics.Models.Generic;
using Envision.Util;
using System.Drawing;
using Envision.Core.Projects;
using Envision.Core.Projects.ProjectGroups;

namespace Envision.Graphics.Render.RenderPasses;

/// <summary> Forward rendering for generic models. </summary>
public class ForwardRendering : IRenderPass
{
    /// <summary> Reference to the engine. </summary>
    public Engine Engine;
    /// <summary> Settings of shadow rendering. </summary>
    public ShadowInternalSettings ShadowSettings;
    /// <summary> Settings of outline rendering. </summary>
    public OutlineInternalSettings OutlineSettings;

    #region Shaders

    /// <summary> The PBR shader used for rendering models. </summary>
    /// <param name="engine"></param>
    private Shader? _pbrShader;
    /// <summary> Shadow mapping depth shader. </summary>
    private Shader? _shadowDepthShader;
    //private Shader? _shadowTestShader;
    private Shader? _shadowDebugQuad;
    private Shader? _debugCascadeShader;
    private Shader? _outlineShader;

    private int _lightFBO;
    private int _lightDepthMaps;
    private int _lightSpaceMatrixUBO;
    #endregion

    private PBRDirectionalLight? DirectionalLight;

    public ForwardRendering(Engine engine)
    {
        Engine = engine;
        ShadowSettings = new()
        {
            DepthMapResolution = 4096,
            LightProjectionTuning = -10.0f,
            ShadowStartDepthNear = 500f,
            ShadowEndDepthFar = 2000.0f,
            ShadowCascadeLevels =
            [
                750,
                1000,
                1500,
                1750,
            ]
        };
        OutlineSettings = new()
        {
            OutlineColor = ColorHelper.ColorFromKnownColor(KnownColor.Blue),
            OutlineScaleFactor = 1.03f
        };
    }

    #region Settings
    public struct ShadowInternalSettings
    {
        /// <summary> Resolution of the shadow map. </summary>
        public int DepthMapResolution;
        /// <summary> The depth at which the shadow map cascades start. </summary>
        public float ShadowStartDepthNear;
        /// <summary> The depth at which the shadow map cascades end. </summary>
        public float ShadowEndDepthFar;
        /// <summary> 
        /// Tune this value to adjust the size of the light projection. Higher values will result in a larger projection. 
        /// </summary>
        public float LightProjectionTuning;
        /// <summary>
        /// Shadow cascade levels this is used to determine the distance of each shadow cascade
        /// where the first level is the closest offering higher resolution 
        /// and the last level is the farthest offering lower resolution.
        /// </summary>
        public float[] ShadowCascadeLevels;
    }

    public struct OutlineInternalSettings
    {
        /// <summary> The scale factor of the outline. </summary>
        public float OutlineScaleFactor;
        /// <summary> The color of the outline. </summary>
        public Vector3 OutlineColor;
    }
    #endregion

    public bool IsEnabled { get; set; }

    #region Loading
    public void Load()
    {
        // ====== Shader loading ======
        if (Engine.ShaderHandler is null) return;
        #region Shadow
        //_shadowDepthShader = Engine.ShaderHandler?.GetShader("ShadowDepth") ?? throw new NullReferenceException("Could not get shadow depth shader.");
        //_shadowDebugQuad = Engine.ShaderHandler?.GetShader("DebugQuad") ?? throw new NullReferenceException("Could not get shadow debug quad shader.");
        //_debugCascadeShader = Engine.ShaderHandler?.GetShader("DebugCascade") ?? throw new NullReferenceException("Could not get debug cascade shader.");
        // ====== Shadow loading ======
        /*_lightFBO = GL.GenFramebuffer();
        _lightDepthMaps = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2DArray, _lightDepthMaps);
        GL.TexImage3D(
            TextureTarget.Texture2DArray,
            0,
            PixelInternalFormat.DepthComponent32f,
            ShadowSettings.DepthMapResolution,
            ShadowSettings.DepthMapResolution,
            ShadowSettings.ShadowCascadeLevels.Length + 1,
            0,
            PixelFormat.DepthComponent,
            PixelType.Float,
            IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
        GraphicsUtil.CheckError("Shadow depth map texture parameter setting");

        float[] borderColor = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureBorderColor, borderColor);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _lightFBO);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Framebuffer, _lightFBO, "Light FBO");
        GraphicsUtil.CheckError("Light FBO binding");
        GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, _lightDepthMaps, 0);
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Texture, _lightDepthMaps, "Light depth map");
        GraphicsUtil.CheckError("Light depth map attachment");

        FramebufferErrorCode err = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (err != FramebufferErrorCode.FramebufferComplete)
        {
            throw new Exception($"Light FBO is not complete: {err}");
        }

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        // Light space matrix UBO
        _lightSpaceMatrixUBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.UniformBuffer, _lightSpaceMatrixUBO);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, _lightSpaceMatrixUBO, "Light space matrix UBO");
        GL.BufferData(BufferTarget.UniformBuffer, Marshal.SizeOf(typeof(Matrix4)) * 16, IntPtr.Zero, BufferUsageHint.StaticDraw);
        GraphicsUtil.CheckError("Light space matrix UBO buffer data");
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, _lightSpaceMatrixUBO);
        GraphicsUtil.CheckError("Light space matrix UBO binding");
        GL.BindBuffer(BufferTarget.UniformBuffer, 0);*/
        #endregion
        #region Outline
        _outlineShader = new(Engine.ShaderHandler, "Outline", "outline");
        GL.Enable(EnableCap.StencilTest);
        GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
        GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
        #endregion
        // ============================

        // ======= PBR loading ========
        _pbrShader = new(Engine.ShaderHandler, "PBR", "pbr");
        for (int i = 0; i < Engine.EngineSettings.MaximumLights; i++)
        {
            Light light = Engine.Lights[i];
            if (light is PBRDirectionalLight pbrDirectionalLight)
            {
                DirectionalLight = pbrDirectionalLight;
            }
        }
        // =============================
    }
    #endregion

    #region Render
    public void Render()
    {
        Project? activeProject = ProjectManager.ActiveProject;
        if (activeProject is null)
        {
            return;
        }

        #region Shadow Pass
        // ============ Shadow pass ============
        /*// Light space matrix UBO setup
        List<Matrix4> lightSpaceMatrices = LightSpaceMatrices();
        GL.BindBuffer(BufferTarget.UniformBuffer, _lightSpaceMatrixUBO);
        GraphicsUtil.CheckError("Light space matrix UBO binding");
        for (int i = 0; i < lightSpaceMatrices.Count; i++)
        {
            Matrix4 lightSpaceMat = lightSpaceMatrices[i];
            lightSpaceMat.Transpose();
            GL.BufferSubData(BufferTarget.UniformBuffer, i * Marshal.SizeOf(typeof(Matrix4)), Marshal.SizeOf(typeof(Matrix4)), ref lightSpaceMat);
            GraphicsUtil.CheckError("Light space matrix UBO buffer sub data");
        }
        GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        if (_shadowDepthShader is null)
        {
            throw new NullReferenceException("Shadow depth shader is not set.");
        }
        _shadowDepthShader.Use();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _lightFBO);
        GL.Viewport(0, 0, ShadowSettings.DepthMapResolution, ShadowSettings.DepthMapResolution);
        GL.Clear(ClearBufferMask.DepthBufferBit);
        GL.CullFace(CullFaceMode.Front);
        foreach (CoreModel model in Engine.Models)
        {
            Matrix4 transformation = model.Transformation;
            _shadowDepthShader.SetMatrix4("model", ref transformation, true);
            foreach (Mesh mesh in model.GetMeshes())
            {
                MeshRenderData? renderData = mesh.RenderData;
                if (renderData is null)
                {
                    continue;
                }
                if (!renderData.IsLoaded)
                {
                    renderData.Load();
                    continue;
                }
                renderData.Render();
            }
        }
        GL.CullFace(CullFaceMode.Back);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);*/
        // ======================================
        #endregion

        // ============ PBR pass =============
        GL.CullFace(CullFaceMode.Back);
        GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
        GL.StencilMask(0xFF);
        if (_pbrShader is null)
        {
            throw new NullReferenceException("PBR shader is not set.");
        }
        _pbrShader.Use();
        /*_pbrShader.SetFloat("farPlane", ref Engine.EngineSettings.DepthFar);
        _pbrShader.SetInt("cascadeCount", ShadowSettings.ShadowCascadeLevels.Length);
        for (int i = 0; i < ShadowSettings.ShadowCascadeLevels.Length; i++)
        {
            _pbrShader.SetFloat($"cascadePlaneDistances[{i}]", ref ShadowSettings.ShadowCascadeLevels[i]);
        }
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2DArray, _lightDepthMaps);
        _pbrShader.SetInt("shadowMap", 0);*/
        int pntLightI = 0;
        for (int i = 0; i < Engine.EngineSettings.MaximumLights; i++)
        {
            Light light = Engine.Lights[i];
            if (light is PBRPointLight pbrPointLight)
            {
                _pbrShader.SetVector3($"pointLights[{pntLightI}].position", ref pbrPointLight.Position);
                _pbrShader.SetVector3($"pointLights[{pntLightI}].color", ref pbrPointLight.LightData.Color);
                _pbrShader.SetFloat($"pointLights[{pntLightI}].intensity", ref pbrPointLight.LightData.Intensity);
                _pbrShader.SetFloat($"pointLights[{pntLightI}].constant", ref pbrPointLight.LightData.Constant);
                _pbrShader.SetFloat($"pointLights[{pntLightI}].linear", ref pbrPointLight.LightData.Linear);
                _pbrShader.SetFloat($"pointLights[{pntLightI}].quadratic", ref pbrPointLight.LightData.Quadratic);
                pntLightI++;
            }
            else if (light is PBRDirectionalLight pbrDirectionalLight)
            {
                _pbrShader.SetVector3($"dirLight.direction", ref pbrDirectionalLight.Position);
                _pbrShader.SetVector3($"dirLight.color", ref pbrDirectionalLight.LightData.Color);
                _pbrShader.SetFloat($"dirLight.intensity", ref pbrDirectionalLight.LightData.Intensity);
            }
        }
        
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
            _pbrShader.SetBool("shadowEnabled", modelRenderData.ShadowEnabled);

            foreach (GenericModelPart corePart in model.Parts)
            {
                Matrix4 meshTransform = corePart.Transformation;
                _pbrShader.SetMatrix4("model", ref meshTransform);
                Matrix3 normalMat = corePart.NormalMatrix();
                _pbrShader.SetMatrix3("normalMatrix", ref normalMat);
                foreach (GenericMesh mesh in corePart.Meshes)
                {
                    if (!mesh.IsLoaded)
                    {
                        mesh.Load();
                        continue;
                    }
                    _pbrShader.SetBool("hasTangents", mesh.HasTangents);

                    GenericMeshShaderData shaderData = mesh.ShaderData;
                    PBRMaterialData pbrMat = shaderData.MaterialData;
                    Texture2D albedo = pbrMat.AlbedoTexture;
                    Texture2D normal = pbrMat.NormalTexture;
                    Texture2D arm = pbrMat.ARMTexture;
                    if (!albedo.Loaded) continue;
                    albedo?.Use(TextureUnit.Texture1);
                    _pbrShader.SetInt("material.albedoMap", 1);
                    if (!normal.Loaded) continue;
                    normal?.Use(TextureUnit.Texture2);
                    _pbrShader.SetInt("material.normalMap", 2);
                    if (!arm.Loaded) continue;
                    arm?.Use(TextureUnit.Texture3);
                    _pbrShader.SetInt("material.ARMMap", 3);

                    mesh.Render();
                }
            }
        }

        #region Outline Pass
        if (_outlineShader is null)
        {
            throw new NullReferenceException("Outline shader is not set.");
        }
        _outlineShader.Use();
        _outlineShader.SetVector3("outlineColor", OutlineSettings.OutlineColor);
        GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
        GL.StencilMask(0x00);
        GL.Disable(EnableCap.DepthTest);
        foreach (GenericModel model in genericModels)
        {
            foreach (GenericModelPart modelPart in model.Parts)
            {
                Matrix4 transform = modelPart.Transformation;
                _outlineShader.SetMatrix4("model", ref transform);
                Matrix3 normalMat = modelPart.NormalMatrix();
                _outlineShader.SetMatrix3("normalMatrix", ref normalMat);
                foreach (GenericMesh mesh in modelPart.Meshes)
                {
                    if (mesh.ShaderData.OutlineData.IsEnabled)
                    {
                        _outlineShader.SetVector3("outlineColor", OutlineSettings.OutlineColor);
                        mesh.Render();
                    }
                }
            }
        }
        GL.Enable(EnableCap.DepthTest);
        GL.StencilMask(0xFF);
        GL.StencilFunc(StencilFunction.Always, 0, 0xFF);
        #endregion

        #region Debug
        /*// Debug cascade
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        _debugCascadeShader?.Use();
        _debugCascadeShader?.SetMatrix4("projection", ref Engine.Projection, false);
        _debugCascadeShader?.SetMatrix4("view", ref view, false);
        DrawCascades(lightSpaceMatrices);
        GL.Disable(EnableCap.Blend);

        float offset_x;
        float offset_y;
        for (int i = 0; i < ShadowSettings.ShadowCascadeLevels.Length + 1; i++)
        {
            offset_x = (i % 3) * 0.6f;
            offset_y = (i / 3) * 0.6f;
            _shadowDebugQuad?.Use();
            _shadowDebugQuad?.SetInt("layer", i);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2DArray, _lightDepthMaps);
            _shadowDebugQuad?.SetInt("depthMap", 0);
            _shadowDebugQuad?.SetFloat("screenDiv", 6);
            Vector3 offset = new(offset_x - 0.5f, offset_y - 0.3f, 0.0f);
            _shadowDebugQuad?.SetVector3("offset", ref offset);
            RenderQuad();
        }*/
        #endregion
        // ======================================
    }
    #endregion

    #region Cascade Shadows Calculations
    private readonly List<int> cascadesVAOs = new(8);
    private readonly List<int> cascadesVBOs = new(8);
    private readonly List<int> cascadesEBOs = new(8);
    private void DrawCascades(List<Matrix4> lightMatrices)
    {
        uint[] indices = new uint[]
        {
            0, 2, 3,
            0, 3, 1,
            4, 6, 2,
            4, 2, 0,
            5, 7, 6,
            5, 6, 4,
            1, 3, 7,
            1, 7, 5,
            6, 7, 3,
            6, 3, 2,
            1, 5, 4,
            0, 1, 4
        };

        List<Vector4> colors = new()
        {
            new(1.0f, 0.0f, 0.0f, 0.3f),
            new(0.0f, 1.0f, 0.0f, 0.3f),
            new(0.0f, 0.0f, 1.0f, 0.3f),
            new(1.0f, 1.0f, 0.0f, 0.3f),
            new(1.0f, 0.0f, 1.0f, 0.3f),
        };

        for (int i = 0; i < lightMatrices.Count; i++)
        {
            List<Vector4> corners = FrustrumCornersWorldSpace(Engine.Projection, lightMatrices[i]);
            List<Vector3> vec3s = [];
            foreach (Vector4 corner in corners)
            {
                vec3s.Add(corner.Xyz);
            }

            if (cascadesVAOs.Count <= i)
            {
                cascadesVAOs.Add(GL.GenVertexArray());
                cascadesVBOs.Add(GL.GenBuffer());
                cascadesEBOs.Add(GL.GenBuffer());
            }

            GL.BindVertexArray(cascadesVAOs[i]);

            GL.BindBuffer(BufferTarget.ArrayBuffer, cascadesVBOs[i]);
            GL.BufferData(BufferTarget.ArrayBuffer, vec3s.Count * sizeof(float) * 3, vec3s.ToArray(), BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, cascadesEBOs[i]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            Vector4 color = colors[i % 3];
            _debugCascadeShader?.SetVector4("color", ref color);
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
        }
    }

    private int _debugQuadVAO = 0;
    private int _debugQuadVBO;
    private void RenderQuad()
    {
        if (_debugQuadVAO == 0)
        {
            float[] quadVertices = {
            // positions        // texture Coords
            -1.0f,  1.0f, 0.0f, 0.0f, 1.0f,
            -1.0f, -1.0f, 0.0f, 0.0f, 0.0f,
             1.0f,  1.0f, 0.0f, 1.0f, 1.0f,
             1.0f, -1.0f, 0.0f, 1.0f, 0.0f,
            };

            GL.GenVertexArrays(1, out _debugQuadVAO);
            GL.GenBuffers(1, out _debugQuadVBO);
            GL.BindVertexArray(_debugQuadVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _debugQuadVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        }
        GL.BindVertexArray(_debugQuadVAO);
        GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        GL.BindVertexArray(0);
    }

    /// <summary>
    /// Returns the corners of the frustrum in world space.
    /// </summary>
    /// <param name="proj"></param>
    /// <param name="view"></param>
    /// <returns></returns>
    public static List<Vector4> FrustrumCornersWorldSpace(Matrix4 projView)
    {
        Matrix4 inverse = projView.Inverted();

        List<Vector4> corners = new();
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                for (int z = 0; z < 2; z++)
                {
                    Vector4 pt = inverse * new Vector4(
                        2.0f * x - 1.0f,
                        2.0f * y - 1.0f,
                        2.0f * z - 1.0f,
                        1);
                    corners.Add(pt /= pt.W);
                }
            }
        }

        return corners;
    }

    public static List<Vector4> FrustrumCornersWorldSpace(Matrix4 proj, Matrix4 view)
    {
        Matrix4 projView = view * proj;
        return FrustrumCornersWorldSpace(projView);
    }

    public Matrix4 LightSpaceMatrix(float nearPlane, float farPlane)
    {
        Matrix4.CreatePerspectiveFieldOfView(
                        MathHelper.DegreesToRadians(Engine.EngineSettings.FieldOfView),
                        ShadowSettings.DepthMapResolution / ShadowSettings.DepthMapResolution,
                        nearPlane,
                        farPlane,
                        out Matrix4 proj);
        // Debug view to see the light space matrix
        //Matrix4 view = Matrix4.LookAt(Vector3.Zero, DirectionalLight?.Position ?? Vector3.Zero, Vector3.UnitY);
        List<Vector4> corners = FrustrumCornersWorldSpace(proj, Engine.Camera.View);
        Vector3 center = Vector3.Zero;
        foreach (Vector4 corner in corners)
        {
            center += corner.Xyz;
        }
        center /= corners.Count;

        Vector3 lightDir = DirectionalLight?.Position ?? Vector3.Zero;
        Matrix4 lightView = Matrix4.LookAt(center + lightDir, center, Vector3.UnitY);

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;
        for (int i = 0; i < corners.Count; i++)
        {
            Vector4 trf = lightView * corners[i];
            minX = Math.Min(minX, trf.X);
            maxX = Math.Max(maxX, trf.X);
            minY = Math.Min(minY, trf.Y);
            maxY = Math.Max(maxY, trf.Y);
            minZ = Math.Min(minZ, trf.Z);
            maxZ = Math.Max(maxZ, trf.Z);
        }

        float zMult = ShadowSettings.LightProjectionTuning;
        if (minZ < 0)
        {
            minZ *= zMult;
        }
        else
        {
            minZ /= zMult;
        }
        if (maxZ < 0)
        {
            maxZ /= zMult;
        }
        else
        {
            maxZ *= zMult;
        }

        Matrix4 lightProj = Matrix4.CreateOrthographicOffCenter(minX, maxX, minY, maxY, -minZ, -maxZ);
        return lightView * lightProj;
    }

    public List<Matrix4> LightSpaceMatrices()
    {
        List<Matrix4> matrices = [];
        float[] shadowCascades = ShadowSettings.ShadowCascadeLevels;
        int cascadeCount = shadowCascades.Length;
        for (int i = 0; i <= cascadeCount; i++)
        {
            if (i == 0)
            {
                matrices.Add(LightSpaceMatrix(ShadowSettings.ShadowStartDepthNear, shadowCascades[i]));
            }
            else if (i < cascadeCount)
            {
                matrices.Add(LightSpaceMatrix(shadowCascades[i - 1], shadowCascades[i]));
            }
            else
            {
                matrices.Add(LightSpaceMatrix(shadowCascades[i - 1], ShadowSettings.ShadowEndDepthFar));
            }
        }
        return matrices;
    }
    #endregion

    public void Dispose()
    {
        _debugCascadeShader?.Dispose();
        _shadowDepthShader?.Dispose();
        _shadowDebugQuad?.Dispose();
        _pbrShader?.Dispose();
        GC.SuppressFinalize(this);
    }
}
