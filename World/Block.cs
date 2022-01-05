using Microsoft.Xna.Framework;
using MyGame.Data;
using MyGame.World.Blocks;

namespace MyGame.World;

public abstract class Block
{
    private const string DefaultTexture = "notex";

    public virtual TextureReference GetTexture() =>
        new()
        {
            Name = DefaultTexture,
            Uv1 = Vector2.Zero,
            Uv2 = Vector2.One
        };

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

    public virtual void OnNeighborUpdated(ref BlockData block, BlockFace direction, BlockData neighbor, WorldManager world)
    {
        var flag = neighbor.Kind is null or AirBlock ? direction : BlockFace.None;
        block.VisibleBlockFaces = (block.VisibleBlockFaces & ~direction) | flag;
    }
}