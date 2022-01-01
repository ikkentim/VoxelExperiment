namespace MyGame.World.Blocks.BlockTypes;

public class AirBlock : BlockBase
{
    public static AirBlock Instance { get; } = new();
    public override TextureReference GetTexture() => new();

    public override void OnCreated(ref BlockData block, IntVector3 position, WorldManager world)
    {
    }

    public override void OnNeighborUpdated(ref BlockData block, Face direction, BlockData neighbor, WorldManager world)
    {
    }
}