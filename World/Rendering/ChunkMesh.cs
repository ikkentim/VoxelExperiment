using Microsoft.Xna.Framework.Graphics;

namespace MyGame.World.Rendering;

public class ChunkMesh
{
    private readonly MeshPart[] _parts;

    private bool _isLines;
    public ChunkMesh(MeshPart[] parts, bool isLines)
    {
        _parts = parts;
        _isLines = isLines;
    }

    public void Render(GraphicsDevice graphicsDevice, BasicEffect basicEffect)
    {
        foreach (var part in _parts)
        {
            graphicsDevice.Indices = part.IndexBuffer;
            graphicsDevice.SetVertexBuffer(part.VertexBuffer);
            basicEffect.Texture = part.Texture;
            
            if (_isLines)
            {
                basicEffect.TextureEnabled = false;
                foreach (var pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, part.PrimitiveCount * 3);
                }
                basicEffect.TextureEnabled = true;
            }
            else
            {
                foreach (var pass in basicEffect.CurrentTechnique.Passes)
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
        public int PrimitiveCount;
    }
}