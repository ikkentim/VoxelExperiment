using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace MyGame.World.Rendering;

public class WorldChunkRendererBySimplePerSquareMeshes : IWorldChunkRenderer
{
    private readonly WorldChunk _chunk;
    private readonly WorldChunkRendererResources _resources;
    
    public WorldChunkRendererBySimplePerSquareMeshes(WorldChunk chunk, WorldChunkRendererResources resources)
    {
        _chunk = chunk;
        _resources = resources;
    }

    private readonly List<SimplePerSquareMesh> _layers = new();

    public void Initialize()
    {
        // initial mesh build
        InitializeLayers(IntVector3.UnitX);
        InitializeLayers(IntVector3.UnitY);
        InitializeLayers(IntVector3.UnitZ);
    }

    private void InitializeLayers(IntVector3 normal)
    {
        // create mesh for every layer (chunk size + 1)
        for (var i = 0; i <= WorldChunk.ChunkSize; i++)
        {
            var layer = new SimplePerSquareMesh(_chunk.ChunkPosition, normal, i);

            layer.Build(_chunk);

            _layers.Add(layer);
        }
    }
    
    public void BlockUpdated(IntVector3 localPosition)
    {
        // rebuild meshes
        throw new NotImplementedException();
    }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        DrawAllMeshLayers(graphicsDevice);
    }
    
    private void DrawAllMeshLayers(GraphicsDevice graphicsDevice)
    {
        foreach (var m in _layers)
        {
            m.Draw(graphicsDevice, _resources.BasicEffect);
        }
    }
}