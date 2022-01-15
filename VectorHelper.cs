using System;
using Microsoft.Xna.Framework;
using MyGame.Data;

namespace MyGame;

public class VectorHelper
{
    public static Vector3 Abs(Vector3 vec) => new(MathF.Abs(vec.X), MathF.Abs(vec.Y), MathF.Abs(vec.Z));

    public static IntVector3 Floor(Vector3 vec) => new((int)MathF.Floor(vec.X), (int)MathF.Floor(vec.Y), (int)MathF.Floor(vec.Z));
}