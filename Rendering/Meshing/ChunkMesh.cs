using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Debugging;

namespace MyGame.Rendering.Meshing;

public class ChunkMesh
{
    private readonly IEnumerable<MeshPart> _parts;
    
    public ChunkMesh(IEnumerable<MeshPart> parts)
    {
        _parts = parts;
    }

    public void Render(GraphicsDevice graphicsDevice, ChunkRendererResources resources)
    {
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
}