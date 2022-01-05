﻿using Microsoft.Xna.Framework;

namespace MyGame.Extensions;

public static class Vector3Extensions
{
    public static Vector3 Normalized(this Vector3 vector)
    {
        vector.Normalize();
        return vector;
    }
}