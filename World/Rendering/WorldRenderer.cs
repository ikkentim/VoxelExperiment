using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Rendering;

namespace MyGame.World.Rendering;

public class WorldRenderer
{
    private readonly WorldManager _world;
    private readonly Camera _camera;
    private readonly WorldChunkRendererResources _rendererResources;
    public WorldRenderer(WorldManager world, Camera camera)
    {
        _world = world;
        _camera = camera;
        _world.Renderer = this;
        _rendererResources = new WorldChunkRendererResources();
    }

    public void Initialize(GraphicsDevice graphicsDevice)
    {
        _rendererResources.Initialize(graphicsDevice);
    }

    public void LoadContent(ContentManager content)
    {
        _rendererResources.LoadContent(content);
    }

    public void ChunkLoaded(WorldChunk chunk)
    {
        chunk.Renderer = new NewWorldChunkRenderer(chunk, _rendererResources);

        chunk.Renderer.Initialize();
    }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        _rendererResources.BasicEffect!.View = _camera.ViewMatrix;
        _rendererResources.BasicEffect.Projection = GlobalGameContext.Current.Projection;

        var chunks = _world.GetChunks();

        foreach (var chunk in chunks)
        {
            chunk.Renderer?.Draw(graphicsDevice);
        }
    }
}