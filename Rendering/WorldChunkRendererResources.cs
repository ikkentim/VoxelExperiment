using Microsoft.Xna.Framework.Graphics;

namespace MyGame.Rendering;

public class WorldChunkRendererResources
{
    private BasicEffect? _basicEffect;
    
    public BasicEffect BasicEffect => _basicEffect!;

    public TextureProvider TextureProvider { get; }

    public WorldChunkRendererResources(TextureProvider textureProvider)
    {
        TextureProvider = textureProvider;
    }

    public void Initialize(GraphicsDevice graphicsDevice)
    {
        _basicEffect = new BasicEffect(graphicsDevice);
        _basicEffect.TextureEnabled = true;
    }
}