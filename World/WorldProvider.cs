using MyGame.Data;

namespace MyGame.World;

public class WorldProvider
{
    private readonly IWorldGenerator _generator = new FlatWorldGenerator();

    public Chunk GetChunk(BlockRegistry blockRegistry, WorldManager world, IntVector3 chunkPosition)
    {
        return _generator.Generate(blockRegistry, world, chunkPosition);
    }
}