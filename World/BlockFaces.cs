using System;
using MyGame.Data;

namespace MyGame.World;

public static class BlockFaces
{
    public const BlockFace PositiveFaces = BlockFace.Top | BlockFace.East | BlockFace.South;
    public const BlockFace NegativeFaces = BlockFace.Bottom | BlockFace.West | BlockFace.North;

    public static IntVector3[] Normals { get; } =
    {
        IntVector3.UnitX,
        IntVector3.UnitY,
        IntVector3.UnitZ,
        -IntVector3.UnitX,
        -IntVector3.UnitY,
        -IntVector3.UnitZ
    };

    public static BlockFace[] AllFaces { get; } =
    {
        BlockFace.East,
        BlockFace.Top,
        BlockFace.South,
        BlockFace.West,
        BlockFace.Bottom,
        BlockFace.North
    };

    public static BlockFace GetOpposite(BlockFace blockFace)
    {
        if ((blockFace & PositiveFaces) != BlockFace.None)
            return (BlockFace)((int)blockFace << 1);

        return (BlockFace)((int)blockFace >> 1);
    }

    public static bool IsPositive(BlockFace blockFace) => BlockFace.None != (PositiveFaces & blockFace);
    public static bool IsNegative(BlockFace blockFace) => BlockFace.None != (NegativeFaces & blockFace);

    public static IntVector3 GetNormal(BlockFace blockFace)
    {
        return blockFace switch
        {
            BlockFace.Bottom => -IntVector3.UnitY,
            BlockFace.Top => IntVector3.UnitY,
            BlockFace.South => IntVector3.UnitZ,
            BlockFace.North => -IntVector3.UnitZ,
            BlockFace.West => -IntVector3.UnitX,
            BlockFace.East => IntVector3.UnitX,
            _ => throw new ArgumentOutOfRangeException(nameof(blockFace), blockFace, null)
        };
    }

    public static IntVector3 GetUp(BlockFace blockFace)
    {
        return blockFace switch
        {
            BlockFace.Bottom => IntVector3.UnitZ,
            BlockFace.Top => IntVector3.UnitZ,
            BlockFace.South => IntVector3.UnitY,
            BlockFace.North => IntVector3.UnitY,
            BlockFace.West => IntVector3.UnitY,
            BlockFace.East => IntVector3.UnitY,
            _ => throw new ArgumentOutOfRangeException(nameof(blockFace), blockFace, null)
        };
    }

    public static BlockFace GetFace(IntVector3 normal)
    {
        if (normal == IntVector3.Zero)
        {
            return BlockFace.None;
        }

        if (normal.X == 0 && normal.Y == 0)
        {
            return normal.Z < 0 ? BlockFace.North : BlockFace.South;
        }

        if (normal.X == 0 && normal.Z == 0)
        {
            return normal.Y < 0 ? BlockFace.Bottom : BlockFace.Top;
        }

        if (normal.Y == 0 && normal.Z == 0)
        {
            return normal.X < 0 ? BlockFace.West : BlockFace.East;
        }

        return BlockFace.None;
    }
}