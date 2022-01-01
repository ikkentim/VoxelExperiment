using Microsoft.Xna.Framework;
using MyGame.World.Blocks.BlockTypes;

namespace MyGame.World.Blocks;

public abstract class BlockBase
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
        var faces = Face.None;

        foreach (var face in Faces.AllFaces)
        {
            var normal = Faces.GetNormal(face);

            var neighbor = position + normal;
            
            if (world.GetBlock(neighbor).Kind is null or AirBlock)
            {
                faces |= face;
            }
        }

        block.VisibleFaces = faces;
    }

    public virtual void OnNeighborUpdated(ref BlockData block, Face direction, BlockData neighbor, WorldManager world)
    {
        var flag = neighbor.Kind is null or AirBlock ? direction : Face.None;
        block.VisibleFaces = (block.VisibleFaces & ~direction) | flag;
    }
}