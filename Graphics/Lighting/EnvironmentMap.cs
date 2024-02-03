using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Envision.Util;
using StbImageSharp;

namespace Envision.Graphics.Lighting;

/// <summary> Class for the environment map which is used for image based lighting in PBR. </summary>
public class EnvironmentMap
{
    public int TextureID;
    public AssetStreamer AssetStreamer;
    public int VertexBufferObject;
    public int VertexArrayObject;
    public string Path;
    /// <summary>
    /// 0 = Right,
    /// 1 = Left,
    /// 2 = Top,
    /// 3 = Bottom,
    /// 4 = Front,
    /// 5 = Back
    /// </summary>
    public string[] Faces;
    public Vector3 Scale;
    public Vector3 Color;

    public EnvironmentMap(string path, AssetStreamer assetStreamer, string[] faces, Vector3 scale, Vector3 color)
    {
        Faces = faces;
        Path = path;
        AssetStreamer = assetStreamer;
        Scale = scale;
        Color = color;
    }

    public void Load()
    {
        string[] facePaths = new string[6];
        for (int i = 0; i < 6; i++)
        {
            facePaths[i] = $"{Path}\\{Faces[i]}";
        }

        void afterFacesLoaded(byte[][] bytes, object? obj)
        {
            TextureID = GL.GenTexture();

            GL.BindTexture(TextureTarget.TextureCubeMap, TextureID);

            for (int i = 0; i < 6; i++)
            {
                ImageResult image = ImageResult.FromMemory(bytes[i], ColorComponents.RedGreenBlueAlpha);
                GL.TexImage2D(
                    TextureTarget.TextureCubeMapPositiveX + i,
                    0,
                    PixelInternalFormat.Rgba,
                    image.Width,
                    image.Height,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    image.Data);
            }
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
        }

        AssetStreamer.StreamingAssetPackage assetPackage = new("Skybox", facePaths, afterFacesLoaded, null);
        AssetStreamer.LoadAsset(assetPackage);

        VertexBufferObject = GL.GenBuffer();
        VertexArrayObject = GL.GenVertexArray();

        GL.BindVertexArray(VertexArrayObject);
        GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);

        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

        GL.EnableVertexAttribArray(0);

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
    }

    public void Render()
    {
        GL.BindVertexArray(VertexArrayObject);
        GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.TextureCubeMap, TextureID);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.DepthFunc(DepthFunction.Less);
    }

    private readonly float[] _vertices =
    {
        -1.0f,  1.0f, -1.0f,
        -1.0f, -1.0f, -1.0f,
         1.0f, -1.0f, -1.0f,
         1.0f, -1.0f, -1.0f,
         1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,

        -1.0f, -1.0f,  1.0f,
        -1.0f, -1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f,  1.0f,
        -1.0f, -1.0f,  1.0f,

         1.0f, -1.0f, -1.0f,
         1.0f, -1.0f,  1.0f,
         1.0f,  1.0f,  1.0f,
         1.0f,  1.0f,  1.0f,
         1.0f,  1.0f, -1.0f,
         1.0f, -1.0f, -1.0f,

        -1.0f, -1.0f,  1.0f,
        -1.0f,  1.0f,  1.0f,
         1.0f,  1.0f,  1.0f,
         1.0f,  1.0f,  1.0f,
         1.0f, -1.0f,  1.0f,
        -1.0f, -1.0f,  1.0f,

        -1.0f,  1.0f, -1.0f,
         1.0f,  1.0f, -1.0f,
         1.0f,  1.0f,  1.0f,
         1.0f,  1.0f,  1.0f,
        -1.0f,  1.0f,  1.0f,
        -1.0f,  1.0f, -1.0f,

        -1.0f, -1.0f, -1.0f,
        -1.0f, -1.0f,  1.0f,
         1.0f, -1.0f, -1.0f,
         1.0f, -1.0f, -1.0f,
        -1.0f, -1.0f,  1.0f,
         1.0f, -1.0f,  1.0f
    };
}
