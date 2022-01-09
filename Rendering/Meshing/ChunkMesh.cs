using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyGame.Rendering.Meshing;

public class ChunkMesh
{
    private readonly IEnumerable<MeshPart> _parts;

    private readonly bool _isLines;
    public ChunkMesh(IEnumerable<MeshPart> parts, bool isLines)
    {
        _parts = parts;
        _isLines = isLines;
    }

    public void Render(GraphicsDevice graphicsDevice, ChunkRendererResources resources)
    {

        foreach (var part in _parts)
        {
            graphicsDevice.Indices = part.IndexBuffer;
            graphicsDevice.SetVertexBuffer(part.VertexBuffer);
            resources.NewEffect.Texture = part.Texture;
            resources.NewEffect.TextureSize = part.TextureSize;
            
            if (_isLines)
            {
                foreach (var pass in resources.BasicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, part.PrimitiveCount * 3);
                }

            }
            else
            {
                foreach (var pass in resources.NewEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, part.PrimitiveCount);
                }
            }
        }
    }

    public struct MeshPart
    {
        public Texture2D Texture;
        public IndexBuffer IndexBuffer;
        public VertexBuffer VertexBuffer;
        public Vector2 TextureSize;
        public int PrimitiveCount;
    }
}