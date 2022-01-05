using Microsoft.Xna.Framework.Graphics;

namespace MyGame.Rendering;

public class ChunkRendererResources
{
    private BasicEffect? _basicEffect;
    
    public BasicEffect BasicEffect => _basicEffect!;

    public TextureProvider TextureProvider { get; }

    public ChunkRendererResources(TextureProvider textureProvider)
    {
        TextureProvider = textureProvider;
    }

    public void Initialize(GraphicsDevice graphicsDevice)
    {
        _basicEffect = new BasicEffect(graphicsDevice);
        _basicEffect.TextureEnabled = true;
    }
}