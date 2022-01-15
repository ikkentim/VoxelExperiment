using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

    public override void Update(GameTime gameTime)
    {
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
    }
}