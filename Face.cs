﻿using System;

namespace MyGame;

[Flags]
public enum Face
{
    /// <summary>
    /// Y+, Up
    /// </summary>
    Top = 1,
    /// <summary>
    /// Y-, Down
    /// </summary>
    Bottom = 2,
    /// <summary>
    /// Z+, Backward
    /// </summary>
    South = 4,
    /// <summary>
    /// Z-, Forward
    /// </summary>
    North = 8,
    /// <summary>
    /// X- Left
    /// </summary>
    West = 16,
    /// <summary>
    /// X+, Right
    /// </summary>
    East = 32,
}
