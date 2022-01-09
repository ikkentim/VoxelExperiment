using System.Collections.Generic;

namespace MyGame.World.Blocks;

public class GrassBlock : Block
{
    public override string Name => "grass";

    public override IEnumerable<TextureReference> GetTextures()
    {
        yield return new TextureReference { Name = "grass_top" };
        yield return new TextureReference { Name = "dirt" };
        yield return new TextureReference { Name = "dirt_grass" };
    }

    public override TextureReference GetTexture(BlockFace face)
    {
        switch (face)
        {
            case BlockFace.Top: return new TextureReference { Name = "grass_top" };
            case BlockFace.Bottom: return new TextureReference { Name = "dirt" };
            default: return new TextureReference { Name = "dirt_grass" };
        }
    }
}