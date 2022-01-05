using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Data;
using MyGame.World;

namespace MyGame.Rendering.Meshing;

public class WorldChunkMeshRender : IWorldChunkRenderer
{
    private const bool IsLines = false; // generate line meshes instead of texture meshes

    private readonly WorldChunkMeshGenerator _meshGenerator;
    private readonly Chunk _chunk;
    private readonly WorldChunkRendererResources _rendererResources;
    private ChunkMesh? _mesh;

    public WorldChunkMeshRender(Chunk chunk, WorldChunkRendererResources rendererResources)
    {
        _chunk = chunk;
        _rendererResources = rendererResources;
        _meshGenerator = new WorldChunkMeshGenerator(chunk, rendererResources.TextureProvider, IsLines);
    }

    public void Initialize(GraphicsDevice graphicsDevice)
    {
        _mesh = _meshGenerator.Create(graphicsDevice);
    }

    public void BlockUpdated(IntVector3 localPosition)
    {
        // TODO
        throw new NotImplementedException();
    }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

        _rendererResources.BasicEffect.World = Matrix.CreateTranslation(_chunk.WorldPosition);

        _mesh!.Render(graphicsDevice, _rendererResources.BasicEffect);
    }
}