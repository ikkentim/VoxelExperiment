using System;
using System.Collections.Generic;
using System.Linq;
using MyGame.World.Rendering;

namespace MyGame.World;

public class WorldManager
{
    private readonly List<WorldChunk> _loadedChunks = new();

    public WorldRenderer? Renderer { get; set; }

    public WorldChunk? GetChunk(IntVector3 position)
    {
        return _loadedChunks.FirstOrDefault(x => x.ChunkPosition == position);
    }

    public bool IsInBounds(IntVector3 position)
    {
        // testworld has 1 chunk and GetBlock is broken with <0...
        return position.X >= 0 && position.Y >= 0 && position.Z >= 0 && position / WorldChunk.ChunkSize == IntVector3.Zero;
    }

    public ref BlockData GetBlock(IntVector3 position)
    {
        // TODO: broken with negative numbers

        var chunkPosition = position / WorldChunk.ChunkSize;
        var chunk = GetChunk(chunkPosition);

        if (chunk == null)
        {
            throw new InvalidOperationException();
        }

        var localPosition = position - chunkPosition;
        ref var blockData = ref chunk.GetBlock(localPosition);

        return ref blockData;
    }

    public IEnumerable<WorldChunk> GetChunks()
    {
        return _loadedChunks;
    }

    private WorldChunk GenerateTestChunk()
    {
        var chunk = new WorldChunk(new IntVector3(0, 0, 0));

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
        var chunk = GenerateTestChunk();

        _loadedChunks.Add(chunk);

        // late notify of creation.. should do better later
        for (var x = 0; x < 16; x++)
        for (var y = 0; y < 02; y++)
        for (var z = 0; z < 16; z++)
            chunk.Blocks[x, y, z].Kind?.OnCreated(ref chunk.Blocks[x, y, z], new IntVector3(x, y, z), this);

    }

    public void UpdateLoadedChunks()
    {
        //
    }
}
