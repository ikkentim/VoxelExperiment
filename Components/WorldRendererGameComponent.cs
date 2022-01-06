using Microsoft.Xna.Framework;
using MyGame.Rendering;

namespace MyGame.Components;

public class WorldRendererGameComponent : DrawableGameComponent
{
    private readonly WorldRenderer _renderer;

    public WorldRendererGameComponent(VoxelGame game) : base(game)
    {
        _renderer = new WorldRenderer(game.WorldManager, game.Camera, game.TextureRegistry);
    }

    public override void Initialize()
    {
        _renderer.Initialize(GraphicsDevice);
    }

    public override void Draw(GameTime gameTime)
    {
        _renderer.Draw(GraphicsDevice);
    }
}