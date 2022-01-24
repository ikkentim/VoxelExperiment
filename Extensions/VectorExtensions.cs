using Microsoft.Xna.Framework;

namespace MyGame.Extensions;

public static class VectorExtensions
{
    public static Vector3 Normalized(this Vector3 vector)
    {
        vector.Normalize();
        return vector;
    }

    public static Vector3 ToXyz(this Vector4 vector)
    {
        return new Vector3(vector.X, vector.Y, vector.Z);
    }
}
