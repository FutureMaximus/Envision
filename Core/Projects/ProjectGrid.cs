using Envision.Graphics.Shaders;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Envision.Core.Projects;

/// <summary> Renderable grid mesh for editors </summary>
public class ProjectGrid
{
    public bool Enabled = true;
    public int Size;
    public int Spacing;
    public int LineWidth;
    public Vector3 Color;

    public InternalGrid InternalData;

    public struct InternalGrid
    {
        public int VAO;
        public int VBO;
        public int EBO;
        public Shader GridShader;

        public InternalGrid(Shader gridShader, int vao, int vbo, int ebo)
        {
            GridShader = gridShader;
            VAO = vao;
            VBO = vbo;
            EBO = ebo;
        }
    }

    public ProjectGrid(int size, int spacing, int lineWidth, Vector3 color)
    {
        Size = size;
        Spacing = spacing;
        LineWidth = lineWidth;
        Color = color;
    }

    public void Load(ref ShaderHandler shaderHandler)
    {
        Shader shader = new(shaderHandler, "Grid", "grid");

        List<Vector3> verts = new();
        List<uint> indices = new();

        for (int i = -Size; i <= Size; i += Spacing)
        {
            verts.Add(new(i, 0, -Size));
            verts.Add(new(i, 0, Size));
            verts.Add(new(-Size, 0, i));
            verts.Add(new(Size, 0, i));
        }
        for (int i = 0; i < verts.Count; i++)
        {
            indices.Add((uint)i);
        }

        int vao = GL.GenVertexArray();
        int vbo = GL.GenBuffer();
        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, verts.Count * sizeof(float) * 3, verts.ToArray(), BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        int ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), indices.ToArray(), BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        GL.BindVertexArray(0);

        InternalData = new(shader, vao, vbo, ebo);
    }

    public void Render()
    {
        if (!Enabled) return;
        InternalData.GridShader.Use();
        Matrix4 identity = Matrix4.Identity;
        InternalData.GridShader.SetMatrix4("model", ref identity);
        GL.BindVertexArray(InternalData.VAO);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, InternalData.EBO);
        GL.LineWidth(LineWidth);
        GL.DrawElements(PrimitiveType.Lines, (InternalData.EBO / sizeof(uint)) * 2, DrawElementsType.UnsignedInt, 0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        GL.BindVertexArray(0);
    }
}
