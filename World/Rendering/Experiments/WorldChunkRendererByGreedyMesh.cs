using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace MyGame.World.Rendering.Experiments;

public class WorldChunkRendererByGreedyMesh : IWorldChunkRenderer
{
    private readonly WorldChunk _chunk;
    private readonly WorldChunkRendererResources _rendererResources;

    public WorldChunkRendererByGreedyMesh(WorldChunk chunk, WorldChunkRendererResources rendererResources)
    {
        _chunk = chunk;
        _rendererResources = rendererResources;
    }

    private List<GreedyMeshForSingleTextureWorld> _layers = new();

    public void Initialize(GraphicsDevice graphicsDevice)
    {
        for (var i = 0; i < WorldChunk.ChunkSize; i++)
        {
            foreach (var face in Faces.AllFaces)
            {
                var mesh = new GreedyMeshForSingleTextureWorld(_chunk, _rendererResources, face, i);
                mesh.Build();
                _layers.Add(mesh);
            }
        }
    }

    public void BlockUpdated(IntVector3 localPosition)
    {
    }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        foreach (var mesh in _layers)
        {
            mesh.Draw(graphicsDevice);
        }
    }
}