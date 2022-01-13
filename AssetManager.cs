using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Rendering.Effects;

namespace MyGame;

public class AssetManager
{
    private BlockFaceEffect? _blockFaceEffectPrefab;
    private SpriteFont? _debugFont;
    public void LoadContent(ContentManager content)
    {
        _blockFaceEffectPrefab = new BlockFaceEffect(content.Load<Effect>("Effects/BlockFaceEffect"));
        _debugFont = content.Load<SpriteFont>("debugfont");
    }

    public BlockFaceEffect CreateBlockFaceEffect() => (BlockFaceEffect)_blockFaceEffectPrefab!.Clone();

    public SpriteFont GetDebugFont() => _debugFont!;
}