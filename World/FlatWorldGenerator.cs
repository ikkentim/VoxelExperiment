using MyGame.Data;

namespace MyGame.World;

public class FlatWorldGenerator : IWorldGenerator
{
    public Chunk Generate(BlockRegistry blockRegistry, WorldManager world, IntVector3 chunkPosition)
    {
        var chunk = new Chunk(world, chunkPosition);

        if (chunkPosition.Y == 0)
        {
            void Set(int x, int y, int z, string block) => chunk.SetBlock(new IntVector3(x, y, z), new BlockState
            {
                BlockType = blockRegistry.GetBlock(block)
            });

            const int height = 4;
            for (var x = 0; x < Chunk.Size; x++)
            for (var y = 0; y < height; y++)
            for (var z = 0; z < Chunk.Size; z++)
                Set(x, y, z, y == height - 1 ? "grass" : "dirt");
        }

        return chunk;
    }
}