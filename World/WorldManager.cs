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
    
    public void LoadChunk(Chunk chunk)
    {
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
