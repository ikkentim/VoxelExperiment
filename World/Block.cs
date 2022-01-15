using System.Collections.Generic;
using MyGame.Data;
using MyGame.World.Blocks;

namespace MyGame.World;

public abstract class Block
{
    public const string DefaultTexture = "notex";

    public virtual string Name => GetType().FullName!;

    public virtual TextureReference GetTexture(BlockFace face) =>
        new()
        {
            Name = DefaultTexture
        };

    public virtual IEnumerable<TextureReference> GetTextures()
    {
        yield return new TextureReference { Name = DefaultTexture };
    }

    public virtual void OnCreated(ref BlockData block, IntVector3 position, WorldManager world)
    {
        var faces = BlockFace.None;

        foreach (var face in BlockFaces.AllFaces)
        {
            var normal = BlockFaces.GetNormal(face);

            var neighbor = position + normal;

            if (world.GetBlock(neighbor).Kind is null or AirBlock)
            {
                faces |= face;
            }
        }

        block.VisibleBlockFaces = faces;
    }

    public virtual bool OnNeighborUpdated(ref BlockData block, BlockFace direction, BlockData neighbor, WorldManager world)
    {
        if (neighbor.Kind is null or AirBlock)
        {
            var before = block.VisibleBlockFaces & direction;
            block.VisibleBlockFaces |= direction;

            return before == 0;
        }
        else
        {
            block.VisibleBlockFaces &= ~direction;
            return false;
        }
    }
}