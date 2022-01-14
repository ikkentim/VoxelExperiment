using System;
using System.Collections.Generic;
using System.Linq;
using MyGame.Data;
using MyGame.Rendering;
using MyGame.World.Blocks;

namespace MyGame.World;

public class WorldManager
{
    private readonly VoxelGame _voxelGame;

    private BlockData _badAirBlock = new()
    {
        Kind = AirBlock.Instance
    };

    private WorldProvider _worldProvider = new();
    private readonly List<Chunk> _loadedChunks = new();
    private readonly Dictionary<IntVector3, Chunk> _chunkByPosition = new();

    private IntVector3? _loadedChunkPosition = null;

    public WorldRenderer? Renderer { get; set; }

    public WorldManager(VoxelGame voxelGame)
    {
        _voxelGame = voxelGame;
    }

    public Chunk? GetChunk(IntVector3 position)
    {
        _chunkByPosition.TryGetValue(position, out var chunk);
        return chunk;
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

        var localPosition = position - chunk.WorldPosition;
        ref var blockData = ref chunk.GetBlock(localPosition);

        return ref blockData;
    }

    public void SetBlock(IntVector3 position, BlockData data)
    {
        var chunkPosition = Chunk.GetChunkPosition(position);

        var chunk = GetChunk(chunkPosition);

        if (chunk == null)
        {
            // Chunk not loaded. Skipping for now.
            return;
        }

        var localPosition = position - chunk.WorldPosition;
        chunk.SetBlock(localPosition, data);
    }

    public IEnumerable<Chunk> GetLoadedChunks()
    {
        return _loadedChunks;
    }
    
    public void LoadChunk(Chunk chunk)
    {
        _loadedChunks.Add(chunk);
        _chunkByPosition[chunk.ChunkPosition] = chunk;
        
        chunk.OnLoaded();

        Renderer?.ChunkLoaded(chunk); 
    }

    public void UnloadChunk(Chunk chunk)
    {
        _chunkByPosition.Remove(chunk.ChunkPosition);
        _loadedChunks.Remove(chunk);

        chunk.Dispose();
    }

    public void UpdateLoadedChunks(IntVector3 playerChunkLocation)
    {
        if (_loadedChunkPosition == null)
        {
            // initial load
            for (var x = -5; x <= 5; x++)
            {
                for (var z = -5; z <= 5; z++)
                {
                    for (var y = 0; y < 16; y++)
                    {
                        var chunk = _worldProvider.GetChunk(_voxelGame.BlockRegistry, this, new IntVector3(x, y, z));
                        LoadChunk(chunk);
                    }
                }
            }

            _loadedChunkPosition = playerChunkLocation;
        }
        else if(_loadedChunkPosition != playerChunkLocation)
        {
            // TODO unload old

            // TODO load new
        }
    }
}

public class WorldProvider
{
    private IWorldGenerator _generator = new FlatWorldGenerator();

    public Chunk GetChunk(BlockRegistry blockRegistry, WorldManager world, IntVector3 chunkPosition)
    {
        return _generator.Generate(blockRegistry, world, chunkPosition);
    }
}

public interface IWorldGenerator
{
    Chunk Generate(BlockRegistry blockRegistry, WorldManager world, IntVector3 chunkPosition);
}

public class FlatWorldGenerator : IWorldGenerator
{
    public Chunk Generate(BlockRegistry blockRegistry, WorldManager world, IntVector3 chunkPosition)
    {
        var chunk = new Chunk(world, chunkPosition);

        if (chunkPosition.Y == 0)
        {
            void Set(int x, int y, int z, string block) => chunk.SetBlock(new IntVector3(x, y, z), new BlockData
            {
                Kind = blockRegistry.GetBlock(block)
            });
            
            const int height = 4;
            for (var x = 0; x < Chunk.Size; x++)
            for (var y = 0; y < height; y++)
            for (var z = 0; z < Chunk.Size; z++)
                Set(x, y, z, y == height - 1 ? "grass" : "dirt");

            if (chunkPosition == IntVector3.Zero)
            {
                Set(7, 3, 7, "cobblestone");
                Set(7, 7, 7, "cobblestone");

                Set(7, 7, 8, "dirt");
                Set(7, 7, 9, "dirt");
                Set(7, 7, 10, "dirt");
            }
        }

        return chunk;
    }
}