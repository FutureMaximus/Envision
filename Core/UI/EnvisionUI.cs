using Envision.Core.UI.SubUI;
using Envision.Graphics.Render;
using ImGuiNET;
using System.Numerics;
using Envision.Graphics.Textures;
using System.Diagnostics;

namespace Envision.Core.UI;

public static class EnvisionUI
{
    private static bool _showNewObjectUI = false;
    private readonly static Type? _newObjectToCreate = null;

    /// <summary>
    /// The main UI for Envision. If you have to use fields or the UI involves methods, use a sub UI.
    /// </summary>
    /// <param name="engine"></param>
    public static void Update(Engine engine)
    {
        ImGuiStylePtr style = ImGui.GetStyle();
        style.FrameRounding = 0.0f;
        style.WindowRounding = 6.0f;
        style.TabRounding = 6.0f;

        // Set tab colors
        style.Colors[(int)ImGuiCol.Tab] = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
        // Set button colors
        style.Colors[(int)ImGuiCol.Button] = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
        // Set window colors
        style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
        // Set frame colors
        style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
        // Set highlight colors
        //style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
        // Set active colors
        style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
        style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.1f, 0.1f, 0.1f, 1.0f);
        style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.1f, 0.1f, 0.1f, 1.0f);
        style.Colors[(int)ImGuiCol.DragDropTarget] = new Vector4(0.1f, 0.1f, 0.9f, 1.0f);
        style.Colors[(int)ImGuiCol.Header] = new Vector4(0.3f, 0.3f, 0.3f, 1.0f);

        #region Top menu bar
        ImGuiWindowFlags menuBarFlags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollWithMouse
            | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoDocking;
        ImGui.Begin("Menu Bar", menuBarFlags);
        ImGui.SetWindowPos(new Vector2(0, 0));
        // Set to full width
        ImGui.SetWindowSize(new Vector2(engine.EngineSettings.WindowSize.X, 50));
        ImGui.SetWindowFontScale(1.3f);
        if (ImGui.Button("File"))
        {
        }
        if (ImGui.Button("Open"))
        {
            Process.Start("explorer.exe");
        }
        ImGui.End();
        #endregion

        #region DockSpace
        ImGuiWindowFlags dockSpaceFlags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoNav
            | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.DockNodeHost;
        ImGui.Begin("DockSpaceArea", dockSpaceFlags);
        ImGui.SetWindowSize(new Vector2(engine.EngineSettings.WindowSize.X, engine.EngineSettings.WindowSize.Y - 50));
        ImGui.SetWindowFontScale(1.0f);
        // Set window position to below the menu bar
        ImGui.SetWindowPos(new Vector2(0, 50));
        ImGui.DockSpace(ImGui.GetID("DockSpace"));
        ImGui.End();
        #endregion

        #region ScreenFBO
        if (engine.ScreenFBO is not null)
        {
            ImGuiWindowFlags flags =
                ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoCollapse;
            ImGui.Begin("Viewport", flags);
            if (!ImGui.IsWindowDocked())
            {
                ImGui.SetWindowPos(new Vector2(0, 50));
                ImGui.SetWindowSize(new Vector2(engine.EngineSettings.WindowSize.X, engine.EngineSettings.WindowSize.Y - 50));
            }
            Texture2D? texture = engine.ScreenFBO.ScreenTexture;
            if (texture is not null)
            {
                // Adjust the screen size to the window size
                OpenTK.Mathematics.Vector2i windowSize = new((int)ImGui.GetWindowSize().X, (int)ImGui.GetWindowSize().Y);
                engine.ScreenFBO.Resize(windowSize);
                ImGui.Image(texture.Handle, ImGui.GetWindowSize(), new(0, 1), new(1, 0));
            }
            ImGui.End();
        }
        #endregion

        #region Project Hierachy
        ProjectHierarchy.Show(ref engine);
        #endregion

        #region SubUIs
        if (_showNewObjectUI)
        {
            CameraControl.CanMove = false;
            NewObjectUI.Show(in engine, ref _showNewObjectUI, _newObjectToCreate);
        }
        else
        {
            CameraControl.CanMove = true;
        }
        #endregion
    }
}
