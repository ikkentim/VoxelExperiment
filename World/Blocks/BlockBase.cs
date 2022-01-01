using Microsoft.Xna.Framework;

namespace MyGame.World.Blocks;

public abstract class BlockBase
{
    public virtual TextureReference GetTexture() =>
        new()
        {
            Name = "notex",
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

            if (!world.IsInBounds(neighbor))
            {
                faces |= face;
            }
            else
            {
                if (world.GetBlock(neighbor).Kind == null) // air
                {
                    faces |= face;
                }
            }
        }

        block.VisibleFaces = faces;
    }

    public virtual void OnNeighborUpdated(ref BlockData block, Face direction, BlockData neighbor, WorldManager world)
    {
        // todo: update
    }
}