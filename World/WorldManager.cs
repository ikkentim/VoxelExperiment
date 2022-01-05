using System;
using System.Collections.Generic;
using System.Linq;
using MyGame.Data;
using MyGame.Rendering;
using MyGame.World.Blocks;

namespace MyGame.World;

public class WorldManager
{
    private BlockData _badAirBlock = new()
    {
        Kind = AirBlock.Instance
    };

    private readonly List<Chunk> _loadedChunks = new();

    public WorldRenderer? Renderer { get; set; }

    public Chunk? GetChunk(IntVector3 position)
    {
        return _loadedChunks.FirstOrDefault(x => x.ChunkPosition == position);
    }
    
    public ref BlockData GetBlock(IntVector3 position)
    {
        var chunkPosition = Chunk.GetChunkPosition(position);

        var chunk = GetChunk(chunkPosition);

        if (chunk == null)
        {
            // Chunk not loaded. Returning a local air block for now.
            // TODO: Should change this later because callers can modify the contents of the block data.
            return ref _badAirBlock;
        }

        var localPosition = position - chunkPosition;
        ref var blockData = ref chunk.GetBlock(localPosition);

        return ref blockData;
    }

    public IEnumerable<Chunk> GetLoadedChunks()
    {
        return _loadedChunks;
    }

    private static void GenerateTestChunk(Chunk chunk)
    {
        var t = new TestBlock();

        var c = new CobbleBlock();

        for (var x = 0; x < Chunk.Size; x++)
        for (var y = 0; y < 2; y++)
        for (var z = 0; z < Chunk.Size; z++)
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
        var chunk = new Chunk(this, new IntVector3(0, 0, 0));
        GenerateTestChunk(chunk);
        
        _loadedChunks.Add(chunk);

        // OnLoaded must be called after the chunk was added to the loaded chunks list to allow the mesh generator to find face information.
        chunk.OnLoaded();

        Renderer?.ChunkLoaded(chunk); 
    }

    public void UpdateLoadedChunks()
    {
        throw new NotImplementedException();
    }
}
