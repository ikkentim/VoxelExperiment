using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Debugging;

namespace MyGame.Rendering.Meshing;

public interface IChunkMesh : IDisposable
{
    void Render(GraphicsDevice graphicsDevice, ChunkRendererResources resources);
}

public sealed class EmptyChunkMesh : IChunkMesh
{
    public static IChunkMesh Instance { get; } = new EmptyChunkMesh();

    public void Dispose()
    {
    }

    public void Render(GraphicsDevice graphicsDevice, ChunkRendererResources resources)
    {
    }
}

public class ChunkMesh : IChunkMesh
{
    private readonly IEnumerable<MeshPart> _parts;
    private bool _isDisposed;

    public ChunkMesh(IEnumerable<MeshPart> parts)
    {
        _parts = parts;
    }

    public void Render(GraphicsDevice graphicsDevice, ChunkRendererResources resources)
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(ChunkMesh));
        }

        var calls = 0;
        
        PerformanceCounters.Drawing.StartMeasurement("chunk_mesh_render");
        foreach (var part in _parts)
        {
            graphicsDevice.Indices = part.IndexBuffer;
            graphicsDevice.SetVertexBuffer(part.VertexBuffer);

            resources.BlockFaceEffect.Texture = part.Texture;
            resources.BlockFaceEffect.TextureSize = part.TextureSize;

            foreach (var pass in resources.BlockFaceEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, part.PrimitiveCount);
                calls++;
            }

            if (part.LineVertexBuffer != null)
            {
                graphicsDevice.Indices = part.LineIndexBuffer;
                graphicsDevice.SetVertexBuffer(part.LineVertexBuffer);

                resources.BlockFaceEffect.DrawLines = true;
                resources.BlockFaceEffect.LineColor = Color.White;


                foreach (var pass in resources.BlockFaceEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, part.LinePrimitiveCount);
                    calls++;
                }

                resources.BlockFaceEffect.DrawLines = false;
            }
        }

        PerformanceCounters.Drawing.StopMeasurement();
        
        PerformanceCounters.Drawing.Add("draw", calls);
    }

    public struct MeshPart
    {
        public Texture2D Texture;
        public IndexBuffer IndexBuffer;
        public VertexBuffer VertexBuffer;
        public IndexBuffer? LineIndexBuffer;
        public VertexBuffer? LineVertexBuffer;
        public Vector2 TextureSize;
        public int PrimitiveCount;
        public int LinePrimitiveCount;
    }
    
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        foreach (var part in _parts)
        {
            part.IndexBuffer.Dispose();
            part.VertexBuffer.Dispose();
            part.LineIndexBuffer?.Dispose();
            part.LineVertexBuffer?.Dispose();
        }

        _isDisposed = true;

        GC.SuppressFinalize(this);
    }
}