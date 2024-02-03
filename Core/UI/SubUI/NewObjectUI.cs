using Envision.Graphics.Models.Generic;
using Envision.Graphics.Render;
using Envision.Util;
using ImGuiNET;
using System.Numerics;

namespace Envision.Core.UI.SubUI;

public static class NewObjectUI
{
    private static byte[] _modelNameInput = new byte[50];
    private static byte[] _modelFilePathInput = new byte[50];

    private static int _modelTextureSizeX = 128;
    private static int _modelTextureSizeY = 128;

    public static void Show(in Engine engine, ref bool show, Type? newObjectToCreate)
    {
        // New window in center of screen
        ImGuiWindowFlags flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.1f, 0.1f, 0.1f, 1f));
        ImGui.Begin("NewObjectUI", flags);
        ImGui.PopStyleColor();
        // ===== Window =====
        int sizeX = engine.Window.ClientSize.X;
        int sizeY = engine.Window.ClientSize.Y;
        ImGui.SetWindowSize(new Vector2(sizeX / 3, sizeY / 4));
        // Set position to center of screen
        ImGui.SetWindowPos(new Vector2((float)sizeX / 3, sizeY / 3));
        // ==================
        // ===== Exit Button =====
        // Exit button with X symbol that is always in the top right corner
        ImGui.SetCursorPos(new Vector2(ImGui.GetWindowSize().X - 40, 5));
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(1, 0, 0, 1f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(1, 0, 0, 1f));
        if (ImGui.Button("X", new Vector2(29, 32)))
        {
            Close(ref show);
        }
        ImGui.PopStyleColor();
        ImGui.PopStyleColor();
        ImGui.PopStyleColor();
        // =======================
        ImGui.End();
    }

    private static void Close(ref bool show)
    {
        _modelFilePathInput = new byte[50];
        _modelNameInput = new byte[50];
        show = false;
    }
}
