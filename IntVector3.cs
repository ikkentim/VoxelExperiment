using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace MyGame;

[DebuggerDisplay("{DebugDisplayString,nq}")]
public readonly struct IntVector3 : IEquatable<IntVector3>
{
    public readonly int X;
    public readonly int Y;
    public readonly int Z;
    
    public IntVector3(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static readonly IntVector3 Zero = new(0, 0, 0);
    public static readonly IntVector3 One = new(1, 1, 1);

    internal string DebugDisplayString => $"({X}, {Y}, {Z})";

    public bool Equals(IntVector3 other)
    {
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    public override bool Equals(object? obj)
    {
        return obj is IntVector3 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }
    
    public static IntVector3 operator *(IntVector3 vec, int scalar) => new(vec.X * scalar, vec.Y * scalar, vec.Z * scalar);

    public static IntVector3 operator /(IntVector3 vec, int scalar) => new(vec.X / scalar, vec.Y / scalar, vec.Z / scalar);

    public static IntVector3 operator +(IntVector3 lhs, IntVector3 rhs) => new(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);

    public static IntVector3 operator -(IntVector3 lhs, IntVector3 rhs) => new(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z);

    public static implicit operator Vector3(IntVector3 vec) => new(vec.X, vec.Y, vec.Z);
    public static bool operator ==(IntVector3 lhs, IntVector3 rhs) => lhs.Equals(rhs);

    public static bool operator !=(IntVector3 lhs, IntVector3 rhs) => !lhs.Equals(rhs);
}