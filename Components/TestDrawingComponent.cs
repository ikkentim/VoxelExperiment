using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Rendering;

namespace MyGame.Components;

public class TestDrawingComponent : DrawableGameComponent
{
    private BasicEffect? _effect;
    public TestDrawingComponent(Game game) : base(game)
    {
    }
    
    private new VoxelGame Game => (VoxelGame)base.Game;

    public override void Initialize()
    {
        _effect = new BasicEffect(GraphicsDevice);
        base.Initialize();
    }
    
    public override void Draw(GameTime gameTime)
    {
        _effect!.View = Game.Camera.ViewMatrix;
        _effect.Projection = GlobalGameContext.Current.Projection;
        _effect.World = Matrix.CreateTranslation(Vector3.One * 2);
        _effect.TextureEnabled = false;
        _effect.VertexColorEnabled = true;

        DrawLine(Vector3.Up, Color.Purple);


        var tx = Game.TextureRegistry.GetTexture("cotton_blue"); // one texture of the bigger atlas
        
        _effect.Texture = tx.Texture;
        _effect.TextureEnabled = true;
        _effect.VertexColorEnabled = false;
        _effect.CurrentTechnique.Passes[0].Apply();

        _effect.World = Matrix.CreateTranslation(new Vector3(4, 6, 4));
        GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, new[]
            {
                new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(4, 0, 0), new Vector2(0, 1)),
                new VertexPositionTexture(new Vector3(0, 4, 0), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(4, 4, 0), new Vector2(0, 0))
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