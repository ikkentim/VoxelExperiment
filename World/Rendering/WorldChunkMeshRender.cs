using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyGame.World.Rendering;

public class WorldChunkMeshRender : IWorldChunkRenderer
{
    private WorldChunkMeshGenerator _meshGenerator;
    private readonly WorldChunk _chunk;
    private readonly WorldChunkRendererResources _rendererResources;
    private ChunkMesh? _mesh;

    public WorldChunkMeshRender(WorldChunk chunk, WorldChunkRendererResources rendererResources)
    {
        _chunk = chunk;
        _rendererResources = rendererResources;
        _meshGenerator = new WorldChunkMeshGenerator(chunk, rendererResources.TextureProvider, true);
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