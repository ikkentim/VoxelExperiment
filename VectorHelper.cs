using System;
using Microsoft.Xna.Framework;

namespace MyGame;

public class VectorHelper
{
    public static Vector3 Abs(Vector3 vec) => new(MathF.Abs(vec.X), MathF.Abs(vec.Y), MathF.Abs(vec.Z));
}