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

        DrawLine(Vector3.Up, Color.Purple);
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