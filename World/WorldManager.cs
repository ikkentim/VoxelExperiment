using System.Collections.Generic;
using System.Linq;

namespace MyGame.World;

public class WorldManager
{
    private readonly List<WorldChunk> _loadedChunks = new();

    public WorldRenderer? Renderer { get; set; }

    public WorldChunk? GetChunk(IntVector3 position)
    {
        return _loadedChunks.FirstOrDefault(x => x.Position == position);
    }

    public IEnumerable<WorldChunk> GetChunks()
    {
        return _loadedChunks;
    }

    private WorldChunk GenerateTestChunk()
    {
        var chunk = new WorldChunk
        {
            Position = new IntVector3(0, 0, 0)
        };

        var t = new TestBlock();

        for (var x = 0; x < 16; x++)
        for (var y = 0; y < 02; y++)
        for (var z = 0; z < 16; z++)
            chunk.SetBlock(new IntVector3(x, y, z), new BlockData
            {
                Kind = t
            });


        Renderer?.ChunkLoaded(chunk);

        return chunk;
    }
    public void LoadInitialChunks()
    {
        _loadedChunks.Add(GenerateTestChunk());
    }

    public void UpdateLoadedChunks()
    {
        //
    }
}
