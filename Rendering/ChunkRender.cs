using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Data;
using MyGame.Debugging;
using MyGame.Rendering.Meshing;
using MyGame.World;

namespace MyGame.Rendering;

public class ChunkRender : IChunkRenderer
{
    private const bool RenderMeshLines = true; // generate line meshes instead of texture meshes

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

    private GraphicsDevice? _graphics;
    public void Initialize(GraphicsDevice graphicsDevice)
    {
        _graphics = graphicsDevice;
        PerformanceCounters.Cumulative.StartMeasurement("mesh_gen");
        _mesh = _meshGenerator.Create(graphicsDevice);
        PerformanceCounters.Cumulative.StopMeasurement();
    }
    
    public void BlockUpdated(IntVector3 localPosition, BlockData oldBlock, BlockData newBlock)
    {
        if (_graphics != null)
        {
            PerformanceCounters.Cumulative.StartMeasurement("mesh_gen");
            _mesh = _meshGenerator.Create(_graphics);
            PerformanceCounters.Cumulative.StopMeasurement();
        }
    }
    
    public void Draw(GraphicsDevice graphicsDevice)
    {
        graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        
        _rendererResources.BlockFaceEffect.World = Matrix.CreateTranslation(_chunk.WorldPosition);

        _mesh!.Render(graphicsDevice, _rendererResources);
    }
    
    public void Dispose()
    {
        _meshGenerator.Dispose();

        GC.SuppressFinalize(this);
    }
}