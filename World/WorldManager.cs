using System.Collections.Generic;
using MyGame.Data;
using MyGame.Rendering;
using MyGame.World.Blocks;

namespace MyGame.World;

public class WorldManager
{
    private readonly Dictionary<IntVector3, Chunk> _chunkByPosition = new();
    private readonly List<Chunk> _loadedChunks = new();
    private readonly VoxelGame _voxelGame;

    private readonly WorldProvider _worldProvider = new();

    private BlockData _badAirBlock = new()
    {
        Kind = AirBlock.Instance
    };

    private IntVector3? _loadedChunkPosition = null;

    public WorldManager(VoxelGame voxelGame)
    {
        _voxelGame = voxelGame;
    }

    public WorldRenderer? Renderer { get; set; }

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
            for (var x = -3; x <= 3; x++)
            {
                for (var z = -3; z <= 3; z++)
                {
                    for (var y = 0; y < 4; y++)
                    {
                        var chunk = _worldProvider.GetChunk(_voxelGame.BlockRegistry, this, new IntVector3(x, y, z));
                        LoadChunk(chunk);
                    }
                }
            }

            _loadedChunkPosition = playerChunkLocation;
        }
        else if (_loadedChunkPosition != playerChunkLocation)
        {
            // TODO unload old

            // TODO load new
        }
    }
}