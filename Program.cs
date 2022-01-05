﻿using System;

namespace MyGame;

public static class Program
{
    [STAThread]
    private static void Main()
    {
        using var game = new VoxelGame();
        game.Run();
    }
}