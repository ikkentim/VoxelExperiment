using MyGame.Rendering.Effects;

namespace MyGame.Rendering;

public class ChunkRendererResources
{
    private BlockFaceEffect? _blockFaceEffect;

    public ChunkRendererResources(TextureRegistry textureRegistry)
    {
        TextureRegistry = textureRegistry;
    }

    public BlockFaceEffect BlockFaceEffect => _blockFaceEffect!;

    public TextureRegistry TextureRegistry { get; }

    public void LoadContent(VoxelGame game)
    {
        _blockFaceEffect = game.AssetManager.CreateBlockFaceEffect();
    }
}