using System;
using System.Diagnostics;

namespace MyGame.World;

[DebuggerDisplay("{DebugDisplayString,nq}")]
public struct BlockState : IEquatable<BlockState>
{
    public Block? BlockType;
    public byte BlockData;
    
    internal string DebugDisplayString => $"({BlockType?.GetType().Name ?? "empty"}:{BlockData})";

    public bool Equals(BlockState other)
    {
        return BlockType == other.BlockType && BlockData == other.BlockData;
    }

    public override bool Equals(object? obj)
    {
        return obj is BlockState other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BlockType, BlockData);
    }

    public static bool operator ==(BlockState lhs, BlockState rhs) => lhs.Equals(rhs);
    public static bool operator !=(BlockState lhs, BlockState rhs) => !lhs.Equals(rhs);
}