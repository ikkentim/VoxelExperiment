using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MyGame.World.Rendering;

public class WorldChunkRendererResources
{
    private BasicEffect? _basicEffect;

    // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
    public BasicEffect BasicEffect => _basicEffect!;

    private Texture2D? _testTexture;

    public void Initialize(GraphicsDevice graphicsDevice)
    {
        _basicEffect = new BasicEffect(graphicsDevice);
        _basicEffect.TextureEnabled = true;
    }

    public void LoadContent(ContentManager content)
    {
        _testTexture = content.Load<Texture2D>("checkered");
        BasicEffect.Texture = _testTexture;
    }
}