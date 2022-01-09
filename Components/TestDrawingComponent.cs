using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Rendering.Effects;
using MyGame.Rendering.Vertices;

namespace MyGame.Components;

public class TestDrawingComponent : DrawableGameComponent
{
    private BasicEffect? _effect;
    private BlockFaceEffect? _blockFaceEffect;

    public TestDrawingComponent(Game game) : base(game)
    {
    }
    
    private new VoxelGame Game => (VoxelGame)base.Game;

    public override void Initialize()
    {
        _effect = new BasicEffect(GraphicsDevice);


        _blockFaceEffect = new BlockFaceEffect(Game.Content.Load<Effect>("BlockFaceEffect"));

        base.Initialize();
    }

    private void ApplyTranslation(IEffectMatrices effect, Vector3 translation)
    {
        effect!.View = Game.Camera.ViewMatrix;
        effect.Projection = GlobalGameContext.Current.Projection;
        effect.World = Matrix.CreateTranslation(translation);
    }

    public override void Draw(GameTime gameTime)
    {
        var cottonBlue = Game.TextureRegistry.GetTexture("cotton_blue"); // one texture of the bigger atlas

        // Draw a simple line
        ApplyTranslation(_effect!, Vector3.One * 2);
        _effect!.TextureEnabled = false;
        _effect.VertexColorEnabled = true;

        DrawLine(Vector3.Up, Color.Purple);


        // Draw with custom effect
        ApplyTranslation(_blockFaceEffect!, Vector3.One * 2);
        _blockFaceEffect!.Texture = cottonBlue.Texture;
        _blockFaceEffect.TextureUv = new Vector2(1f / 5, 1);
        _blockFaceEffect.TextureSize = new Vector2(1f / 5, 1);
        
        foreach (var pass in _blockFaceEffect!.CurrentTechnique.Passes)
        {
            pass.Apply();
        }

        Draw4X1(new Vector2(4, 1));

        
        // Draw texture atlas with basic effect
        ApplyTranslation(_effect!, new Vector3(4, 6, 4));
        _effect!.Texture = cottonBlue.Texture;
        _effect.TextureEnabled = true;
        _effect.VertexColorEnabled = false;
        _effect.CurrentTechnique.Passes[0].Apply();
        
        Draw4X1Basic(Vector2.One);
    }

    private void Draw4X1(Vector2 uv)
    {
        GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, new[]
            {
                new VertexPositionBlockFace(new Vector3(0, 0, 0), uv * new Vector2(1, 1), new Vector2(4f/5, 0)),
                new VertexPositionBlockFace(new Vector3(4, 0, 0), uv * new Vector2(0, 1), new Vector2(4f/5, 0)),
                new VertexPositionBlockFace(new Vector3(0, 1, 0), uv * new Vector2(1, 0), new Vector2(4f/5, 0)),
                new VertexPositionBlockFace(new Vector3(4, 1, 0), uv * new Vector2(0, 0), new Vector2(4f/5, 0))
            },
            0, 4, new short[] { 0, 3, 2, 0, 1, 3 }, 0, 2);
    }

    private void Draw4X1Basic(Vector2 uv)
    {
        GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, new[]
            {
                new VertexPositionTexture(new Vector3(0, 0, 0), uv * new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(4, 0, 0), uv * new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(0, 1, 0), uv * new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(4, 1, 0), uv * new Vector2(0, 0))
            },
            0, 4, new short[] { 0, 3, 2, 0, 1, 3 }, 0, 2);
    }

    private void DrawLine(Vector3 normal, Color c)
    {
        _effect!.CurrentTechnique.Passes[0].Apply();

        GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList,
            new[]
            {
                new VertexPositionColor(Vector3.Zero, c),
                new VertexPositionColor(normal, c),
            },
            0,
            2,
            new short[] { 0, 1 }, 0, 1);
    }

}