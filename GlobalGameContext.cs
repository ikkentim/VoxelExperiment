using Microsoft.Xna.Framework;

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
        500f);

    public static void Initialize(VoxelGame game)
    {
        _current = new GlobalGameContext(game);
    }
}