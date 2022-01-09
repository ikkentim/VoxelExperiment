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
    
    protected override void LoadContent()
    {
        _renderer.LoadContent(Game.Content);
    }

    public override void Draw(GameTime gameTime)
    {
        _renderer.Draw(GraphicsDevice);
    }
}