using System;
using MyGame.World.Blocks;
using MyGame.World.Rendering;

namespace MyGame.World;

public class WorldChunk
{
    public const int ChunkSize = 16;
    
    public IntVector3 ChunkPosition { get; }
    public IntVector3 WorldPosition => ChunkPosition * ChunkSize;

    public BlockData[,,] Blocks { get; } = new BlockData[ChunkSize,ChunkSize,ChunkSize];

    public IWorldChunkRenderer? Renderer { get; set; }
    
    public WorldChunk(IntVector3 chunkPosition)
    {
        ChunkPosition = chunkPosition;
    }

    private void AssertPositionWithinBounds(IntVector3 localPos)
    {
        if (localPos.X < 0 || localPos.Y < 0 || localPos.Z < 0 || localPos.X >= ChunkSize || localPos.Y >= ChunkSize || localPos.Z >= ChunkSize)
        {
            throw new ArgumentOutOfRangeException(nameof(localPos));
        }
    }

    public WorldChunk GetNeighbor(Face direction)
    {
        // test world has only 1 chunk.
        return null;
    }
    
    public IntVector3 GetBlockPosition(IntVector3 localPos)
    {
        AssertPositionWithinBounds(localPos);
        return WorldPosition + localPos;
    }

    public void SetBlock(IntVector3 localPos, BlockData block)
    {
        AssertPositionWithinBounds(localPos);
        Blocks[localPos.X, localPos.Y, localPos.Z] = block;

        Renderer?.BlockUpdated(localPos);
    }

    public ref BlockData GetBlock(IntVector3 localPos)
    {
        AssertPositionWithinBounds(localPos);
        return ref Blocks[localPos.X, localPos.Y, localPos.Z];
    }
}