using MyGame.Data;

namespace MyGame.World.Blocks;

public class AirBlock : Block
{
    public static AirBlock Instance { get; } = new();
    public override TextureReference GetTexture() => new();

    public override void OnCreated(ref BlockData block, IntVector3 position, WorldManager world)
    {
    }

    public override void OnNeighborUpdated(ref BlockData block, BlockFace direction, BlockData neighbor, WorldManager world)
    {
    }
}