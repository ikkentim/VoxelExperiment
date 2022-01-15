using System;
using System.Diagnostics;
using MyGame.Data;
using MyGame.Rendering;
using MyGame.World.Blocks;

namespace MyGame.World;

public class Chunk : IDisposable
{
    public const int Size = 16;

    private readonly BlockData[,,] _blocks = new BlockData[Size, Size, Size];

    private bool _isLoading = true;

    public Chunk(WorldManager world, IntVector3 chunkPosition)
    {
        World = world;
        ChunkPosition = chunkPosition;
    }

    public WorldManager World { get; }
    public IntVector3 ChunkPosition { get; }
    public IntVector3 WorldPosition => ChunkPosition * Size;
    public IChunkRenderer? Renderer { get; set; }

    public void Dispose()
    {
        Renderer?.Dispose();

        GC.SuppressFinalize(this);
    }

    public void OnLoaded()
    {
        if (!_isLoading)
            throw new InvalidOperationException();

        _isLoading = false;

        for (var x = 0; x < Size; x++)
        for (var y = 0; y < Size; y++)
        for (var z = 0; z < Size; z++)
        {
            ref var block = ref GetBlock(new IntVector3(x, y, z));
            if (block.Kind is not null and not AirBlock)
                block.Kind.OnCreated(ref block, WorldPosition + new IntVector3(x, y, z), World);
        }
    }

    public Chunk? GetNeighbor(BlockFace direction) =>
        World.GetChunk(ChunkPosition + BlockFaces.GetNormal(direction));

    public IntVector3 GetBlockPosition(IntVector3 localPos)
    {
        AssertPositionWithinBounds(localPos);
        return WorldPosition + localPos;
    }

    public void SetBlock(IntVector3 localPos, BlockData block)
    {
        AssertPositionWithinBounds(localPos);

        block.Kind ??= AirBlock.Instance;

        var oldBlock = _blocks[localPos.X, localPos.Y, localPos.Z];
        _blocks[localPos.X, localPos.Y, localPos.Z] = block;

        if (_isLoading)
            return;

        block.Kind.OnCreated(ref _blocks[localPos.X, localPos.Y, localPos.Z], WorldPosition + localPos, World);

        foreach (var face in BlockFaces.AllFaces)
        {
            ref var n = ref GetNeighbor(localPos, face, out var chunk);

            if (chunk != null)
            {
                if (n.Kind is not null and not AirBlock)
                {
                    if (n.Kind.OnNeighborUpdated(ref n, BlockFaces.GetOpposite(face), block, World))
                    {
                        chunk.Renderer?.BlockUpdated(localPos + BlockFaces.GetNormal(face) + WorldPosition - chunk.WorldPosition, default,
                            default); // TODO: this is terrible.
                    }
                }
            }
        }

        block = _blocks[localPos.X, localPos.Y, localPos.Z];
        Renderer?.BlockUpdated(localPos, oldBlock, block);
    }

    public ref BlockData GetBlock(IntVector3 localPos)
    {
        AssertPositionWithinBounds(localPos);

        ref var block = ref _blocks[localPos.X, localPos.Y, localPos.Z];
        block.Kind ??= AirBlock.Instance;
        return ref block;
    }

    public ref BlockData GetNeighbor(IntVector3 localPos, BlockFace blockFace, out Chunk? chunk) =>
        ref GetNeighbor(localPos, BlockFaces.GetNormal(blockFace), out chunk);

    public ref BlockData GetNeighbor(IntVector3 localPos, IntVector3 normal, out Chunk? chunk)
    {
        AssertPositionWithinBounds(localPos);

        localPos += normal;
        var relativeChunk = GetChunkPosition(localPos);

        if (relativeChunk == IntVector3.Zero)
        {
            chunk = this;
            return ref GetBlock(localPos);
        }

        // Double getting the chunk here... This method is garbage in the first place.
        // TODO: this is horrible.
        chunk = World.GetChunk(ChunkPosition + relativeChunk);

        return ref World.GetBlock(WorldPosition + localPos);
    }

    private static int GetChunkPositionComponent(int component)
    {
        var chunkComponent = component / Size;

        // offset by 1 for negative values
        if (component < 0 && component % Size != 0)
            chunkComponent--;

        return chunkComponent;
    }

    public static IntVector3 GetChunkPosition(IntVector3 position) =>
        new(
            GetChunkPositionComponent(position.X),
            GetChunkPositionComponent(position.Y),
            GetChunkPositionComponent(position.Z));

    [Conditional("DEBUG")]
    private static void AssertPositionWithinBounds(IntVector3 localPos)
    {
        if (localPos.X < 0 || localPos.Y < 0 || localPos.Z < 0 || localPos.X >= Size || localPos.Y >= Size || localPos.Z >= Size)
        {
            throw new ArgumentOutOfRangeException(nameof(localPos));
        }
    }
}