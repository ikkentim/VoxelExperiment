using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Data;
using MyGame.Rendering.Meshing;
using MyGame.World;

namespace MyGame.Rendering;

public class ChunkRender : IChunkRenderer
{
    private const bool RenderMeshLines = false; // generate line meshes instead of texture meshes

    private readonly GreedyMeshGenerator _meshGenerator;
    private readonly Chunk _chunk;
    private readonly ChunkRendererResources _rendererResources;
    private ChunkMesh? _mesh;

    public ChunkRender(Chunk chunk, ChunkRendererResources rendererResources)
    {
        _chunk = chunk;
        _rendererResources = rendererResources;
        _meshGenerator = new GreedyMeshGenerator(chunk, rendererResources.TextureRegistry, RenderMeshLines);
    }

    public void Initialize(GraphicsDevice graphicsDevice)
    {
        _mesh = _meshGenerator.Create(graphicsDevice);
    }

    public void BlockUpdated(IntVector3 localPosition, BlockData oldBlock, BlockData newBlock)
    {
        // TODO
        throw new NotImplementedException();
    }
    
    public void Draw(GraphicsDevice graphicsDevice)
    {
        graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        
        _rendererResources.BlockFaceEffect.World = Matrix.CreateTranslation(_chunk.WorldPosition);

        _mesh!.Render(graphicsDevice, _rendererResources);
    }
}