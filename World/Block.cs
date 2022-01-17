using System.Collections.Generic;
using MyGame.Data;
using MyGame.World.Blocks;

namespace MyGame.World;

public abstract class Block
{
    public const string DefaultTexture = "notex";

    public virtual string Name => GetType().FullName!;

    public virtual bool IsOpaque => true;
    
    public virtual TextureReference GetTexture(BlockFace face) =>
        new()
        {
            Name = DefaultTexture
        };

    public virtual IEnumerable<TextureReference> GetTextures()
    {
        yield return new TextureReference { Name = DefaultTexture };
    }
}