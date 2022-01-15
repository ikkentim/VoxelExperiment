using Microsoft.Xna.Framework.Graphics;
using MyGame.World;

namespace MyGame.Rendering;

public class WorldRenderer
{
    private readonly Camera _camera;
    private readonly ChunkRendererResources _rendererResources;
    private readonly WorldManager _world;

    public WorldRenderer(WorldManager world, Camera camera, TextureRegistry textureRegistry)
    {
        _world = world;
        _camera = camera;
        _world.Renderer = this;
        _rendererResources = new ChunkRendererResources(textureRegistry);
    }

    public void LoadContent(VoxelGame game)
    {
        _rendererResources.LoadContent(game);
    }

    public void ChunkLoaded(Chunk chunk)
    {
        chunk.Renderer = new ChunkRender(chunk, _rendererResources);
        chunk.Renderer.Initialize(GlobalGameContext.Current.Game.GraphicsDevice);
    }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        _rendererResources.BlockFaceEffect.View = _camera.ViewMatrix;
        _rendererResources.BlockFaceEffect.Projection = GlobalGameContext.Current.Projection;

        var chunks = _world.GetLoadedChunks();

        foreach (var chunk in chunks)
        {
            chunk.Renderer?.Draw(graphicsDevice);
        }
    }
}