using Microsoft.Xna.Framework;

namespace MyGame.Rendering;

public class GlobalGameContext
{
    private static GlobalGameContext? _current;

    private GlobalGameContext(Game1 game)
    {
        Game = game;
    }

    public static GlobalGameContext Current => _current!;

    public GameWindow Window => Game.Window;

    public Game1 Game { get; init; }

    public Vector2 WindowSize => Window.ClientBounds.Size.ToVector2();

    public Matrix Projection => Matrix.CreatePerspectiveFieldOfView(
        MathHelper.PiOver4, // 90 fov
        Window.ClientBounds.Width / (float)Window.ClientBounds.Height,
        0.1f,
        100);

    public static void Initialize(Game1 game)
    {
        _current = new GlobalGameContext(game);
    }
}