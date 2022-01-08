using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Components;

namespace MyGame.Rendering;

public class ChunkRendererResources
{
    private BlockFaceEffect? _newEffect;
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
    }

    public void LoadContent(ContentManager content)
    {
        _newEffect = new BlockFaceEffect(content.Load<Effect>("BlockFaceEffect"));
    }
}