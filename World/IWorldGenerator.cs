using MyGame.Data;

namespace MyGame.World;

public interface IWorldGenerator
{
    Chunk Generate(BlockRegistry blockRegistry, WorldManager world, IntVector3 chunkPosition);
}