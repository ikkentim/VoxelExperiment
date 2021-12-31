namespace MyGame.World;

public abstract class BlockBase
{
    public int Texture { get; } // Texture index
    
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