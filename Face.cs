using System;

namespace MyGame;

[Flags]
public enum Face
{
    /// <summary>
    /// Y+
    /// </summary>
    Top = 1,
    /// <summary>
    /// Y-
    /// </summary>
    Bottom = 2,
    /// <summary>
    /// Z+
    /// </summary>
    North = 4,
    /// <summary>
    /// Z-
    /// </summary>
    South = 8,
    /// <summary>
    /// X-
    /// </summary>
    West = 16,
    /// <summary>
    /// X+
    /// </summary>
    East = 32,
}