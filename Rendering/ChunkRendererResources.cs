using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Components;
using MyGame.Rendering.Effects;

namespace MyGame.Rendering;

public class ChunkRendererResources
{
    private BlockFaceEffect? _blockFaceEffect;
    private BasicEffect? _basicEffect;
    
    public BasicEffect BasicEffect => _basicEffect!;

    public BlockFaceEffect BlockFaceEffect => _blockFaceEffect;

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
        _blockFaceEffect = new BlockFaceEffect(content.Load<Effect>("BlockFaceEffect"));
    }
}