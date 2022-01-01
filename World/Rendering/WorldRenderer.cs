using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Rendering;
using MyGame.World.Rendering.Experiments;

namespace MyGame.World.Rendering;

public class WorldRenderer
{
    private readonly WorldManager _world;
    private readonly Camera _camera;
    private readonly WorldChunkRendererResources _rendererResources;
    public WorldRenderer(WorldManager world, Camera camera, TextureProvider textureProvider)
    {
        _world = world;
        _camera = camera;
        _world.Renderer = this;
        _rendererResources = new WorldChunkRendererResources(textureProvider);
    }

    public void Initialize(GraphicsDevice graphicsDevice)
    {
        _rendererResources.Initialize(graphicsDevice);
    }
    
    public void ChunkLoaded(WorldChunk chunk)
    {
        chunk.Renderer = new WorldChunkRendererByGreedyMesh(chunk, _rendererResources);

        chunk.Renderer.Initialize();
    }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        _rendererResources.BasicEffect.Texture = _rendererResources.TextureProvider.GetTexture("notex");
        _rendererResources.BasicEffect.View = _camera.ViewMatrix;
        _rendererResources.BasicEffect.Projection = GlobalGameContext.Current.Projection;

        var chunks = _world.GetChunks();

        foreach (var chunk in chunks)
        {
            chunk.Renderer?.Draw(graphicsDevice);
        }
    }
}