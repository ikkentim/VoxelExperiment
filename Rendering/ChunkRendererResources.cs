using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Rendering.Effects;

namespace MyGame.Rendering;

public class ChunkRendererResources
{
    private BlockFaceEffect? _blockFaceEffect;

    public BlockFaceEffect BlockFaceEffect => _blockFaceEffect!;

    public TextureRegistry TextureRegistry { get; }

    public ChunkRendererResources(TextureRegistry textureRegistry)
    {
        TextureRegistry = textureRegistry;
    }
    
    public void LoadContent(ContentManager content)
    {
        _blockFaceEffect = new BlockFaceEffect(content.Load<Effect>("Effects/BlockFaceEffect"));
    }
}