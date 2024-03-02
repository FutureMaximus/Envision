using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Mathematics;
using Assimp;
using Assimp.Unmanaged;

namespace Envision.Graphics.Animations;

public class Bone
{
    public string Name { get; }
    public int Index;
    public Matrix4 LocalTransform = Matrix4.Identity;

    public List<KeyframeData<Vector3>> PositionData { get; } = [];
    public List<KeyframeData<OpenTK.Mathematics.Quaternion>> RotationData { get; } = [];
    public List<KeyframeData<Vector3>> ScaleData { get; } = [];
    public uint NumberOfPositionKeys;
    public uint NumberOfRotationKeys;
    public uint NumberOfScaleKeys;

    public Bone(string name, int id, in AiNodeAnim channel)
    {
        Name = name;
        Index = id;
        GetKeys(channel);
    }

    public void Update(float animationTime)
    {

    }

    /// <summary>
    /// Gets the current position index of the bone to interpolate
    /// based on the current animation time.
    /// </summary>
    /// <param name="animationTime"></param>
    /// <returns>
    /// The index of the current position keyframe to interpolate.
    /// </returns>
    public int GetPositionIndex(float animationTime)
    {
        for (int i = 0; i < PositionData.Count - 1; i++)
        {
            if (animationTime < PositionData[i + 1].Time)
            {
                return i;
            }
        }
        return 0;
    }

    /// <summary>
    /// Gets the current rotation index of the bone to interpolate
    /// based on the current animation time.
    /// </summary>
    /// <param name="animationTime"></param>
    /// <returns>
    /// The index of the current rotation keyframe to interpolate.
    /// </returns>
    public int GetRotationIndex(float animationTime)
    {
        for (int i = 0; i < RotationData.Count - 1; i++)
        {
            if (animationTime < RotationData[i + 1].Time)
            {
                return i;
            }
        }
        return 0;
    }

    /// <summary>
    /// Gets the current scale index of the bone to interpolate
    /// based on the current animation time.
    /// </summary>
    /// <param name="animationTime"></param>
    /// <returns>
    /// The index of the current scale keyframe to interpolate.
    /// </returns>
    public int GetScaleIndex(float animationTime)
    {
        for (int i = 0; i < ScaleData.Count - 1; i++)
        {
            if (animationTime < ScaleData[i + 1].Time)
            {
                return i;
            }
        }
        return 0;
    }

    private unsafe void GetKeys(in AiNodeAnim channel)
    {
        NumberOfPositionKeys = channel.NumPositionKeys;
        nint PositionKeys = channel.PositionKeys;
        NumberOfRotationKeys = channel.NumRotationKeys;
        nint RotationKeys = channel.RotationKeys;
        NumberOfScaleKeys = channel.NumScalingKeys;
        nint ScalingKeys = channel.ScalingKeys;

        for (uint i = 0; i < NumberOfPositionKeys; i++)
        {
            VectorKey key = MemoryMarshal.Read<VectorKey>(
                new ReadOnlySpan<byte>(PositionKeys.ToPointer(), 
                (int)NumberOfPositionKeys * Unsafe.SizeOf<VectorKey>()).Slice((int)i * Unsafe.SizeOf<VectorKey>(), 
                Unsafe.SizeOf<VectorKey>()));
            PositionData.Add(new KeyframeData<Vector3>(key.Time, new Vector3(key.Value.X, key.Value.Y, key.Value.Z)));
        }
        for (uint i = 0; i < NumberOfRotationKeys; i++)
        {
            QuaternionKey key = MemoryMarshal.Read<QuaternionKey>(
                new ReadOnlySpan<byte>(RotationKeys.ToPointer(),
                (int)NumberOfRotationKeys * Unsafe.SizeOf<QuaternionKey>()).Slice((int)i * Unsafe.SizeOf<QuaternionKey>(),
                Unsafe.SizeOf<QuaternionKey>()));
            RotationData.Add(new KeyframeData<OpenTK.Mathematics.Quaternion>(
                key.Time, 
                new OpenTK.Mathematics.Quaternion(key.Value.X, key.Value.Y, key.Value.Z, key.Value.W)));
        }
        for (uint i = 0; i < NumberOfRotationKeys; i++)
        {
            VectorKey key = MemoryMarshal.Read<VectorKey>(
                new ReadOnlySpan<byte>(ScalingKeys.ToPointer(),
                (int)NumberOfRotationKeys * Unsafe.SizeOf<VectorKey>()).Slice((int)i * Unsafe.SizeOf<VectorKey>(),
                Unsafe.SizeOf<VectorKey>()));
            ScaleData.Add(new KeyframeData<Vector3>(key.Time, new Vector3(key.Value.X, key.Value.Y, key.Value.Z)));
        }
    }
}
