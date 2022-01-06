using Microsoft.Xna.Framework.Graphics;

namespace MyGame.Rendering;

public class ChunkRendererResources
{
    private BasicEffect? _basicEffect;
    
    public BasicEffect BasicEffect => _basicEffect!;

    public TextureRegistry TextureRegistry { get; }

    public ChunkRendererResources(TextureRegistry textureRegistry)
    {
        TextureRegistry = textureRegistry;
    }

    public void Initialize(GraphicsDevice graphicsDevice)
    {
        _basicEffect = new BasicEffect(graphicsDevice);
        _basicEffect.TextureEnabled = true;
    }
}