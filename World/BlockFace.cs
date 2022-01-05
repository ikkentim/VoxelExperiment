using System;

namespace MyGame.World;

[Flags]
public enum BlockFace : byte
{
    None = 0,

    /// <summary>
    /// X+, Right
    /// </summary>
    East = 1,

    /// <summary>
    /// X- Left
    /// </summary>
    West = 2,

    /// <summary>
    /// Y+, Up
    /// </summary>
    Top = 4,

    /// <summary>
    /// Y-, Down
    /// </summary>
    Bottom = 8,

    /// <summary>
    /// Z+, Backward
    /// </summary>
    South = 16,

    /// <summary>
    /// Z-, Forward
    /// </summary>
    North = 32,
}
