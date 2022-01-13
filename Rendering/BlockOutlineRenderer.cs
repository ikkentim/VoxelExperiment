using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Data;
using MyGame.Rendering.Effects;
using MyGame.World;

namespace MyGame.Rendering;

public class BlockOutlineRenderer
{
    private readonly VoxelGame _game;
    private IndexBuffer? _indexBuffer;
    private VertexBuffer? _vertexBuffer;
    private BlockFaceEffect? _effect;

    public BlockOutlineRenderer(VoxelGame game)
    {
        _game = game;
    }

    public void LoadContent()
    {
        _indexBuffer = new IndexBuffer(_game.GraphicsDevice, IndexElementSize.SixteenBits, 24, BufferUsage.WriteOnly);
        _vertexBuffer = new VertexBuffer(_game.GraphicsDevice, typeof(VertexPosition), 8, BufferUsage.WriteOnly);

        const float lo = -0.01f;
        const float hi = 1.01f;
        _vertexBuffer.SetData(new[]
        {
            new VertexPosition(new Vector3(lo, lo, lo)),
            new VertexPosition(new Vector3(hi, lo, lo)),
            new VertexPosition(new Vector3(lo, hi, lo)),
            new VertexPosition(new Vector3(hi, hi, lo)),
            new VertexPosition(new Vector3(lo, lo, hi)),
            new VertexPosition(new Vector3(hi, lo, hi)),
            new VertexPosition(new Vector3(lo, hi, hi)),
            new VertexPosition(new Vector3(hi, hi, hi)),
        });

        _indexBuffer.SetData(new short[]
        {
            0, 1, 1, 5, 5, 4, 4, 0, 0, 2, 1, 3, 5, 7, 4, 6, 2, 3, 3, 7, 7, 6, 6, 2
        });

        _effect = _game.AssetManager.CreateBlockFaceEffect();
        _effect.DrawLines = true;
        _effect.LineColor = Color.Black;
    }

    public void Render(IntVector3 position, BlockData block)
    {
        _effect!.World = Matrix.CreateTranslation(position);
        _effect.View = _game.Camera.ViewMatrix;
        _effect.Projection = GlobalGameContext.Current.Projection;

        _game.GraphicsDevice.SetVertexBuffer(_vertexBuffer);
        _game.GraphicsDevice.Indices = _indexBuffer;

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, 12);
        }
    }
}