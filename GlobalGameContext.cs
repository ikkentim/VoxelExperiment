﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyGame;

public class GlobalGameContext
{
    private static GlobalGameContext? _current;

    private GlobalGameContext(VoxelGame game)
    {
        Game = game;
    }

    public static GlobalGameContext Current => _current!;

    public GameWindow Window => Game.Window;

    public VoxelGame Game { get; init; }

    public Vector2 WindowSize => Window.ClientBounds.Size.ToVector2();
    
    public Matrix Projection => Matrix.CreatePerspectiveFieldOfView(
        MathHelper.PiOver4, // 90 fov
        Window.ClientBounds.Width / (float)Window.ClientBounds.Height,
        0.1f,
        256f);

    public RenderTarget2D RenderTarget { get; set; }

    public static void Initialize(VoxelGame game)
    {
        _current = new GlobalGameContext(game);
    }
}