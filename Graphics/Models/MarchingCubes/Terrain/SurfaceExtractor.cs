﻿using OpenTK.Mathematics;

namespace Envision.Graphics.Models.MarchingCubes.Terrain;

public interface ISurfaceExtractor
{
    void GenLodRegion(ref TerrainMesh mesh, Vector3i min, int size, int lod);
}

/// <summary>
/// Based on https://transvoxel.org// by Eric Lengyel with
/// modifications for parallelization.
/// </summary>
public class TransvoxelExtractor(IVolumeData<sbyte> data) : ISurfaceExtractor
{
    private readonly IVolumeData<sbyte> _volume = data;

    public void GenLodRegion(ref TerrainMesh mesh, Vector3i min, int size, int lod)
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    Vector3i position = new(x, y, z);
                    PolygonizeCell(min, position, ref mesh, lod);
                }
            }
        }
    }

    public void PolygonizeCell(Vector3i offsetPos, Vector3i pos, ref TerrainMesh mesh, int lod)
    {
        //Debug.Assert(lod >= 1, "Level of Detail must be greater than 1");
        offsetPos += pos * lod;

        sbyte[] density = new sbyte[8];

        for (int i = 0; i < density.Length; i++)
        {
            density[i] = _volume[offsetPos + LengyelTables.CornerIndex[i] * lod];
        }

        byte caseCode = GetCaseCode(density);
        if ((caseCode ^ density[7] >> 7 & 0xFF) == 0) // If there is no triangulation
            return;

        Vector3[] cornerNormals = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            Vector3 p = offsetPos + LengyelTables.CornerIndex[i] * lod;
            float nx = (_volume[p + Vector3i.UnitX] - _volume[p - Vector3i.UnitX]) * 0.5f;
            float ny = (_volume[p + Vector3i.UnitY] - _volume[p - Vector3i.UnitY]) * 0.5f;
            float nz = (_volume[p + Vector3i.UnitZ] - _volume[p - Vector3i.UnitZ]) * 0.5f;

            cornerNormals[i].X = nx;
            cornerNormals[i].Y = ny;
            cornerNormals[i].Z = nz;
            cornerNormals[i].Normalize();
        }

        byte regularCellsClass = LengyelTables.RegularCellClass[caseCode];
        ushort[] vertexLocations = LengyelTables.RegularVertexData[caseCode];

        LengyelTables.RegularCell c = LengyelTables.RegularCellData[regularCellsClass];
        long vertexCount = c.GetVertexCount();
        long triangleCount = c.GetTriangleCount();
        byte[] indexOffset = c.Indizes();
        ushort[] mappedIndices = new ushort[indexOffset.Length];

        for (int i = 0; i < vertexCount; i++)
        {
            byte v1 = (byte)(vertexLocations[i] & 0x0F); //Second Corner Index
            byte v0 = (byte)(vertexLocations[i] >> 4 & 0x0F); //First Corner Index

            sbyte d0 = density[v0];
            sbyte d1 = density[v1];

            //Debug.Assert(v1 > v0);

            int t = (d1 << 8) / (d1 - d0);
            int u = 0x0100 - t;
            float t0 = t / 256f;
            float t1 = u / 256f;

            int index = -1;

            if (index == -1)
            {
                Vector3 normal = cornerNormals[v0] * t0 + cornerNormals[v1] * t1;
                GenerateVertex(ref offsetPos, ref mesh, lod, t, ref v0, ref v1, normal);
                index = mesh.LatestAddedVertIndex();
            }

            mappedIndices[i] = (ushort)index;
        }

        for (int t = 0; t < triangleCount; t++)
        {
            for (int i = 0; i < 3; i++)
            {
                mesh.Indices.Add(mappedIndices[c.Indizes()[t * 3 + i]]);
            }
        }
    }

    private static void GenerateVertex(ref Vector3i offsetPos, ref TerrainMesh mesh, int lod, long t, ref byte v0, ref byte v1, Vector3 normal)
    {
        Vector3i iP0 = offsetPos + LengyelTables.CornerIndex[v0] * lod;
        Vector3 P0 = new(iP0.X, iP0.Y, iP0.Z);

        Vector3i iP1 = offsetPos + LengyelTables.CornerIndex[v1] * lod;
        Vector3 P1 = new(iP1.X, iP1.Y, iP1.Z);

        Vector3 Q = InterpolateVoxelVector(t, P0, P1);

        mesh.Vertices.Add(Q);
        mesh.Normals.Add(normal);
    }

    public static Vector3 InterpolateVoxelVector(long t, Vector3 P0, Vector3 P1)
    {
        long u = 0x0100 - t; //256 - t
        float s = 1.0f / 256.0f;
        Vector3 Q = P0 * t + P1 * u; //Density Interpolation
        Q *= s; // shift to shader !
        return Q;
    }

    private static byte GetCaseCode(sbyte[] density)
    {
        byte code = 0;
        byte konj = 0x01;
        for (int i = 0; i < density.Length; i++)
        {
            code |= (byte)(density[i] >> density.Length - 1 - i & konj);
            konj <<= 1;
        }

        return code;
    }
}
