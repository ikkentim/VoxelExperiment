using System;
using System.Diagnostics;
using MyGame.World.Blocks;
using MyGame.World.Blocks.BlockTypes;
using MyGame.World.Rendering;

namespace MyGame.World;

public class WorldChunk
{
    private bool _isLoading = true;
    public const int ChunkSize = 16;

    public WorldManager World { get; }
    public IntVector3 ChunkPosition { get; }
    public IntVector3 WorldPosition => ChunkPosition * ChunkSize;
    public BlockData[,,] Blocks { get; } = new BlockData[ChunkSize,ChunkSize,ChunkSize];
    public IWorldChunkRenderer? Renderer { get; set; }
    
    public WorldChunk(WorldManager world, IntVector3 chunkPosition)
    {
        World = world;
        ChunkPosition = chunkPosition;
    }

    [Conditional("DEBUG")]
    private void AssertPositionWithinBounds(IntVector3 localPos)
    {
        if (localPos.X < 0 || localPos.Y < 0 || localPos.Z < 0 || localPos.X >= ChunkSize || localPos.Y >= ChunkSize || localPos.Z >= ChunkSize)
        {
            throw new ArgumentOutOfRangeException(nameof(localPos));
        }
    }

    public WorldChunk? GetNeighbor(Face direction) =>
        World.GetChunk(ChunkPosition + Faces.GetNormal(direction));

    public IntVector3 GetBlockPosition(IntVector3 localPos)
    {
        AssertPositionWithinBounds(localPos);
        return WorldPosition + localPos;
    }

    public void SetBlock(IntVector3 localPos, BlockData block)
    {
        AssertPositionWithinBounds(localPos);

        block.Kind ??= AirBlock.Instance;

        Blocks[localPos.X, localPos.Y, localPos.Z] = block;

        if (_isLoading)
            return;

        block.Kind.OnCreated(ref Blocks[localPos.X, localPos.Y, localPos.Z], ChunkPosition + localPos, World);

        foreach (var face in Faces.AllFaces)
        {
            ref var n = ref GetNeighbor(localPos, face);

            if (n.Kind is not null and not AirBlock)
            {
                n.Kind.OnNeighborUpdated(ref n, Faces.GetOpposite(face), block, World);
            }
        }

        Renderer?.BlockUpdated(localPos);
    }

    public void OnLoaded()
    {
        if (!_isLoading)
            throw new InvalidOperationException();

        _isLoading = false;

        for (var x = 0; x < ChunkSize; x++)
        for (var y = 0; y < ChunkSize; y++)
        for (var z = 0; z < ChunkSize; z++)
        {
            if (Blocks[x, y, z].Kind is not null and not AirBlock)
                Blocks[x, y, z].Kind!.OnCreated(ref Blocks[x, y, z], WorldPosition + new IntVector3(x, y, z), World);
        }
    }

    public ref BlockData GetBlock(IntVector3 localPos)
    {
        AssertPositionWithinBounds(localPos);

        ref var block = ref Blocks[localPos.X, localPos.Y, localPos.Z];
        block.Kind ??= AirBlock.Instance;
        return ref block;
    }

    public ref BlockData GetNeighbor(IntVector3 localPos, Face face) => ref GetNeighbor(localPos, Faces.GetNormal(face));

    public ref BlockData GetNeighbor(IntVector3 localPos, IntVector3 normal)
    {
        localPos += normal;
        var relativeChunk = GetChunkPosition(localPos);

        if (relativeChunk == IntVector3.Zero)
            return ref GetBlock(localPos);

        return ref World.GetBlock(WorldPosition + localPos);
    }

    private static int GetChunkPositionComponent(int component)
    {
        var chunkComponent = component / ChunkSize;
        
        // offset by 1 for negative values
        if (component < 0 && component % ChunkSize != 0)
            chunkComponent--;

        return chunkComponent;
    }

    public static IntVector3 GetChunkPosition(IntVector3 position) =>
        new(
            GetChunkPositionComponent(position.X),
            GetChunkPositionComponent(position.Y),
            GetChunkPositionComponent(position.Z));
}