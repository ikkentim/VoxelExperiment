﻿using Microsoft.Xna.Framework.Graphics;
using MyGame.Rendering;

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
        chunk.Renderer = 
            new WorldChunkMeshRender(chunk, _rendererResources);
            //new WorldChunkRendererByGreedyMesh(chunk, _rendererResources);
        chunk.Renderer.Initialize(GlobalGameContext.Current.Game.GraphicsDevice);
    }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        _rendererResources.BasicEffect.TextureEnabled = true;
        _rendererResources.BasicEffect.Texture = _rendererResources.TextureProvider.GetTexture("notex");
        _rendererResources.BasicEffect.View = _camera.ViewMatrix;
        _rendererResources.BasicEffect.Projection = GlobalGameContext.Current.Projection;
      
        var chunks = _world.GetLoadedChunks();

        foreach (var chunk in chunks)
        {
            chunk.Renderer?.Draw(graphicsDevice);
        }
    }
}