using Envision.Graphics.Shaders;
using Envision.Graphics.Textures;
using Envision.Util;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Envision.Graphics.Shaders.Data;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Envision.Graphics.Lighting;
using Envision.Graphics.Render.RenderPasses;
using Envision.Core.UI;
using Envision.Graphics.Render.RenderPasses.SubPasses;
using Envision.Graphics.Lighting.Lights;
using Envision.Physics;
using Envision.Core.Projects;

namespace Envision.Graphics.Render;

public class Engine
{
    /// <summary> Window that owns this engine. </summary>
    public OpenTK.Windowing.Desktop.GameWindow Window { get; set; }
    /// <summary> Handles shaders in the engine. </summary>
    public ShaderHandler? ShaderHandler;
    public Camera Camera { get; } = new();
    public Matrix4 Projection;
    public Matrix4 Orthographic;
    /// <summary> Uniform buffer object that contains the projection, view, and view position. </summary>
    public int ProjViewUBO;
    public Settings EngineSettings;

    public ScreenFBO? ScreenFBO;
    //public EnvironmentMap? SkyBox;

    public readonly List<IRenderPass> RenderPasses = new();

    /// <summary> Streamed assets that may have tasks to be ran on the main thread </summary>
    public readonly ConcurrentStack<IAssetHolder> StreamedAssets = new();
    public readonly AssetStreamer AssetStreamer;

    public List<Light> Lights;

    public PhysicsSimulation PhysicsSimulation { get; } = new();

    public float DeltaTime { get; set; }
    public float TimeElapsed
    {
        get => _timeElapsed;
        set
        {
            _timeElapsed = value;
            if (_timeElapsed >= float.MaxValue)
            {
                _timeElapsed = 0.0f;
            }
        }
    }
    private float _timeElapsed = 0.0f;
    public float FPS { get; set; }

    public Engine(Window window)
    {
        Window = window;
        EngineSettings = new()
        {
            UseDeferredRendering = false,
            UseForwardRendering = true,
            UseDebugRendering = false,
            UseOrthographic = false,
            MaximumLights = 8,
            FieldOfView = 45f,
            AspectRatio = 1f,
            DepthNear = 0.1f,
            DepthFar = 10000f,
            ClearColor = new float[] { 0.05f, 0.05f, 0.05f, 1.0f }
        };
        Lights = new(EngineSettings.MaximumLights);
        AssetStreamer = new(this);
        DeltaTime = 0.0f;
        TimeElapsed = 0.0f;
    }

    public struct Settings
    {
        public bool UseDeferredRendering;
        public bool UseForwardRendering;
        public bool UseDebugRendering;
        public bool UseOrthographic;
        public int MaximumLights { get; set; }
        public float FieldOfView;
        public Vector2i WindowSize;
        public Vector2i WindowPosition;
        public float AspectRatio;
        public float DepthNear;
        public float DepthFar;
        public float[] ClearColor;
    }

    /// <summary>
    /// Loads the engine and its components.
    /// This should be called after the window is created.
    /// </summary>
    public void Load()
    {
        PBRLightData lightData = new()
        {
            Color = new Vector3(1.0f, 1.0f, 1.0f),
            Intensity = 5.0f,
        };
        PBRDirectionalLight directionalLight = new(new Vector3(0.5f, 1.0f, 0.0f), lightData);
        Lights.Add(directionalLight);

        GraphicsUtil.CheckKHRSupported("GL_KHR_debug");
        if (GraphicsUtil.KHRDebugSupported)
        {
            GL.Enable(EnableCap.DebugOutput);
            //GL.Enable(EnableCap.DebugOutputSynchronous); Will block the main thread but can be useful.
            Debug.Print("KHR_debug supported enabled debug mode.");
        }

        ForwardRendering forwardRendering = new(this)
        {
            IsEnabled = EngineSettings.UseForwardRendering
        };
        RenderPasses.Add(forwardRendering);

        // If a texture is non-existing or invalid, use this instead.
        TextureEntries.AddTexture(TextureHelper.GenerateTextureNotFound());

        GL.ClearColor(
            EngineSettings.ClearColor[0],
            EngineSettings.ClearColor[1],
            EngineSettings.ClearColor[2],
            EngineSettings.ClearColor[3]
            );

        if (Window is not null)
        {
            EngineSettings.AspectRatio = Window.Size.X / (float)Window.Size.Y;
            EngineSettings.WindowSize = Window.Size;
            EngineSettings.WindowPosition = new(0, 0);
        }

        if (EngineSettings.UseOrthographic)
        {
            Projection = Matrix4.CreateOrthographicOffCenter(
                -EngineSettings.AspectRatio,
                 EngineSettings.AspectRatio,
                 -1.0f,
                 1.0f,
                 EngineSettings.DepthNear,
                 EngineSettings.DepthFar
                 );
            Orthographic = Projection;
        }
        else
        {
            Projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(EngineSettings.FieldOfView),
                EngineSettings.AspectRatio,
                EngineSettings.DepthNear,
                EngineSettings.DepthFar
                );
        }

        // ============= UBO (Global Shader Data) =============
        ProjViewUBO = GL.GenBuffer();
        // 64 bytes for projection matrix, 64 bytes for view matrix, 16 bytes for camera position.
        GL.BindBuffer(BufferTarget.UniformBuffer, ProjViewUBO);
        GraphicsUtil.LabelObject(ObjectLabelIdentifier.Buffer, ProjViewUBO, "ProjViewUBO Location 0");
        int projViewUBOSize = Marshal.SizeOf(typeof(Matrix4)) * 2 + Marshal.SizeOf(typeof(Vector3));
        GL.BufferData(BufferTarget.UniformBuffer, projViewUBOSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        GraphicsUtil.CheckError("UBO 0 (ProjView) Buffer Data");

        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, ProjViewUBO);
        GraphicsUtil.CheckError("UBO 0 (ProjView) Buffer Base");

        ProjViewUniform projViewUniform = new(Projection, Camera.View, Camera.Position);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, Marshal.SizeOf(typeof(Matrix4)), ref projViewUniform.Projection);
        GL.BufferSubData(BufferTarget.UniformBuffer, Marshal.SizeOf(typeof(Matrix4)), Marshal.SizeOf(typeof(Matrix4)), ref projViewUniform.View);
        GL.BufferSubData(BufferTarget.UniformBuffer, Marshal.SizeOf(typeof(Matrix4)) * 2, Marshal.SizeOf(typeof(Vector3)), ref projViewUniform.ViewPos);
        GraphicsUtil.CheckError("UBO 0 (ProjView) Error");

        GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        // ====================================================

        ShaderHandler = new(Config.Settings.ShaderPath);
        Shader screenFBOShader = new(ShaderHandler, "ScreenFBO", "screen");
        ScreenFBO = new(screenFBOShader, EngineSettings.WindowSize);
        ScreenFBO.Load();

        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);

        Project defaultProject = Project.CreateDefaultProject();
        ProjectManager.Projects.Add(defaultProject);
        ProjectManager.ActiveProject = defaultProject;
        ProjectManager.ActiveProject?.Load();

        foreach (IRenderPass renderPass in RenderPasses)
        {
            if (renderPass.IsEnabled)
                renderPass.Load();
        }
    }

    public void Render()
    {
        // ============= Update =============
        if (EngineSettings.UseOrthographic)
        {
            Projection = Matrix4.CreateOrthographicOffCenter(
                -EngineSettings.AspectRatio,
                 EngineSettings.AspectRatio,
                 -1.0f,
                 1.0f,
                 EngineSettings.DepthNear,
                 EngineSettings.DepthFar
                 );
            Orthographic = Projection;
        }
        else
        {
            Projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(EngineSettings.FieldOfView),
                EngineSettings.AspectRatio,
                EngineSettings.DepthNear,
                EngineSettings.DepthFar
                );
        }
        EnvisionUI.Update(this);

        if (Window is not null)
        {
            EngineSettings.WindowSize = Window.Size;
            EngineSettings.AspectRatio = Window.Size.X / (float)Window.Size.Y;
        }
        // ==================================

        // ============= UBO (Global Shader Data) =============
        GL.BindBuffer(BufferTarget.UniformBuffer, ProjViewUBO);

        ProjViewUniform projViewUniform = new(Projection, Camera.View, Camera.Position);
        GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, Marshal.SizeOf(typeof(Matrix4)), ref projViewUniform.Projection);
        GL.BufferSubData(BufferTarget.UniformBuffer, Marshal.SizeOf(typeof(Matrix4)), Marshal.SizeOf(typeof(Matrix4)), ref projViewUniform.View);
        GL.BufferSubData(BufferTarget.UniformBuffer, Marshal.SizeOf(typeof(Matrix4)) * 2, Marshal.SizeOf(typeof(Vector3)), ref projViewUniform.ViewPos);
        GraphicsUtil.CheckError("UBO 0 (ProjView) Error");
        // ====================================================

        // ============= Render ==============
        ScreenFBO?.Bind();
        GL.ClearColor(
            EngineSettings.ClearColor[0],
            EngineSettings.ClearColor[1],
            EngineSettings.ClearColor[2],
            EngineSettings.ClearColor[3]
            );
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        GL.Enable(EnableCap.DepthTest);

        foreach (IRenderPass renderPass in RenderPasses)
        {
            if (renderPass.IsEnabled)
                renderPass.Render();
        }

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.Disable(EnableCap.DepthTest);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        ScreenFBO?.Render();
        // ===================================
    }

    public void Update(float deltaTime)
    {
        DeltaTime = deltaTime;
        TimeElapsed += deltaTime;

        List<IAssetHolder> streamedAssets = StreamedAssets.ToList() ?? new();
        foreach (IAssetHolder asset in streamedAssets)
        {
            if (asset is AssetStreamer.StreamingAsset streamingAsset)
            {
                if (streamingAsset.Loaded)
                {
                    byte[]? bytes = streamingAsset.Data;
                    if (bytes is null)
                    {
                        RemoveAsset();
                        continue;
                    }
                    streamingAsset.AfterLoadedExecute(bytes, streamingAsset.AssetObjectData);
                    RemoveAsset();
                    continue;
                }
            }
            else if (asset is AssetStreamer.StreamingAssetPackage streamingAssetPackage)
            {
                if (streamingAssetPackage.Loaded)
                {
                    byte[][]? bytes = streamingAssetPackage.Data;
                    if (bytes is null)
                    {
                        RemoveAsset();
                        continue;
                    }
                    streamingAssetPackage.AfterLoadedExecute(bytes, streamingAssetPackage.AssetObjectData);
                    RemoveAsset();
                    continue;
                }
            }
            RemoveAsset();
        }
    }

    private void RemoveAsset()
    {
        if (StreamedAssets.TryPop(out IAssetHolder? poppedAsset))
        {
            poppedAsset.Dispose();
        }
    }

    public void ShutDown()
    {
        foreach (Project project in ProjectManager.Projects)
        {
            // TODO: Save projects then dispose.
            project.Dispose();
        }
        foreach (IRenderPass renderPass in RenderPasses)
        {
            renderPass.Dispose();
        }
        ScreenFBO?.Dispose();
        ShaderHandler?.Dispose();
        GL.DeleteBuffer(ProjViewUBO);
        TextureEntries.Dispose();
    }
}
