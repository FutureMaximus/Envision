using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;

namespace Envision.Util;

/// <summary> Reference for error code information https://registry.khronos.org/OpenGL-Refpages/gl4/. </summary>
public static class GraphicsUtil
{
    public static bool KHRDebugSupported => _debug;
    private static bool _debug = false;

    [Conditional("DEBUG")]
    public static void CheckKHRSupported(string extension)
    {
        int major = GL.GetInteger(GetPName.MajorVersion);
        int minor = GL.GetInteger(GetPName.MinorVersion);

        string[] extensions = GL.GetString(StringName.Extensions).Split(' ');
        foreach (string ext in extensions)
        {
            if (ext == extension && major >= 4 && minor >= 6)
            {
                _debug = true;
            }
        }
    }

    [Conditional("DEBUG")]
    public static void LabelObject(ObjectLabelIdentifier objLabelIdent, int glObject, string name)
    {
        if (KHRDebugSupported)
            GL.ObjectLabel(objLabelIdent, glObject, name.Length, name);
    }

    /// <summary>Checks errors when debug is enabled.</summary>
    /// <param name="callerLocationLabel">A simple text string describing the source calling location.</param>
    /// <param name="context">An optional context object.</param>
    [Conditional("DEBUG")]
    public static void CheckError(string title)
    {
        ErrorCode error;
        int i = 1;
        while ((error = GL.GetError()) != ErrorCode.NoError)
        {
            DebugLogger.Log($"<red>OpenGL Error {title} {i++}: {error}. See reference for error code information.");
            Debug.WriteLine($"OpenGL Error {title} {i++}: {error}. See reference for error code information.");
        }
    }

    /// <summary> The maximum amount of bone influences a vertex can have. </summary>
    public readonly static int MaxBoneInfluence = 4;

    public static int[] EmptyBoneIDs()
    {
        int[] boneIds = new int[MaxBoneInfluence];
        for (int i = 0; i < MaxBoneInfluence; i++)
        {
            boneIds[i] = -1;
        }

        return boneIds;
    }

    public static float[] EmptyBoneWeights()
    {
        float[] boneWeights = new float[MaxBoneInfluence];
        for (int i = 0; i < MaxBoneInfluence; i++)
        {
            boneWeights[i] = 0;
        }

        return boneWeights;
    }
}

