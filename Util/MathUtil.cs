using OpenTK.Mathematics;

namespace Envision.Util;

public static class MathUtil
{
    /// <summary>
    /// The absolute value of a vector.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public static Vector3 VecAbs(float x, float y, float z) => new(Math.Abs(x), Math.Abs(y), Math.Abs(z));
}
