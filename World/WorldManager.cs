using System;
using System.Collections.Generic;
using System.Linq;
using MyGame.World.Blocks;
using MyGame.World.Blocks.BlockTypes;
using MyGame.World.Rendering;

namespace MyGame.World;

public class WorldManager
{
    private BlockData _badAirBlock = new()
    {
        Kind = AirBlock.Instance
    };

    private readonly List<WorldChunk> _loadedChunks = new();

    public WorldRenderer? Renderer { get; set; }

    public WorldChunk? GetChunk(IntVector3 position)
    {
        return _loadedChunks.FirstOrDefault(x => x.ChunkPosition == position);
    }
    
    public ref BlockData GetBlock(IntVector3 position)
    {
        var chunkPosition = WorldChunk.GetChunkPosition(position);

        var chunk = GetChunk(chunkPosition);

        if (chunk == null)
        {
            // Chunk not loaded. Returning a local air block for now. Should change this later because callers can modify the contents of the block data.
            return ref _badAirBlock;
        }

        var localPosition = position - chunkPosition;
        ref var blockData = ref chunk.GetBlock(localPosition);

        return ref blockData;
    }

    public IEnumerable<WorldChunk> GetLoadedChunks()
    {
        return _loadedChunks;
    }

    private void GenerateTestChunk(WorldChunk chunk)
    {
        var t = new TestBlock();

        var c = new CobbleBlock();

        for (var x = 0; x < WorldChunk.ChunkSize; x++)
        for (var y = 0; y < 2; y++)
        for (var z = 0; z < WorldChunk.ChunkSize; z++)
            chunk.SetBlock(new IntVector3(x, y, z), new BlockData
            {
                Kind = t
            });
        
        chunk.SetBlock(new IntVector3(7, 2, 7), new BlockData
        {
            Kind = c
        });
        chunk.SetBlock(new IntVector3(7, 7, 7), new BlockData
        {
            Kind = c
        });
    }

    public void LoadInitialChunks()
    {
        var chunk = new WorldChunk(this, new IntVector3(0, 0, 0));
        GenerateTestChunk(chunk);
        
        _loadedChunks.Add(chunk);

        chunk.OnLoaded();

        Renderer?.ChunkLoaded(chunk); 
    }

    public void UpdateLoadedChunks()
    {
        throw new NotImplementedException();
    }
}
