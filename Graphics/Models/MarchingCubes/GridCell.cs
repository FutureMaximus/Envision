using OpenTK.Mathematics;

namespace Envision.Graphics.Models.MarchingCubes;

/// <summary> Grid cell for marching cubes. </summary>
public struct GridCell
{
    public int X, Y, Z;
    public Vector3[] Vertices;
    public float[] Values;

    public GridCell(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
        Vertices = new Vector3[8];
        Values = new float[8];
    }

    public readonly override bool Equals(object? obj)
    {
        if (obj is GridCell cell)
        {
            return cell.X == X && cell.Y == Y && cell.Z == Z;
        }
        return false;
    }

    public readonly override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    public static bool operator ==(GridCell left, GridCell right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GridCell left, GridCell right)
    {
        return !(left == right);
    }
}
