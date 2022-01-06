using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MyGame.World.Blocks;

public class CobbleBlock : SolidBlock
{
    public CobbleBlock() : base("cobblestone", "cobble")
    {
    }
}

public class SolidBlock : Block
{
    private readonly string _texture;

    public SolidBlock(string name, string texture)
    {
        Name = name;
        _texture = texture;
    }
    
    public override string Name { get; }

    public override TextureReference GetTexture(BlockFace face)
    {
        return new TextureReference { Name = _texture };
    }

    public override IEnumerable<TextureReference> GetTextures()
    {
        yield return new TextureReference { Name = _texture };
    }
}