using System;

namespace MyGame.World;

public class WorldChunk
{
    public const int ChunkSize = 16;
    public IntVector3 Position { get; init; }
    public BlockData[,,] Blocks { get; } = new BlockData[ChunkSize,ChunkSize,ChunkSize];

    public WorldChunkRenderer? Renderer { get; set; }

    private void AssertPositionWithinBounds(IntVector3 localPos)
    {
        if (localPos.X < 0 || localPos.Y < 0 || localPos.Z < 0 || localPos.X >= ChunkSize || localPos.Y >= ChunkSize || localPos.Z >= ChunkSize)
        {
            throw new ArgumentOutOfRangeException(nameof(localPos));
        }
    }

    public IntVector3 GetBlockPosition(IntVector3 localPos)
    {
        AssertPositionWithinBounds(localPos);
        return Position * ChunkSize + localPos;
    }

    public void SetBlock(IntVector3 localPos, BlockData block)
    {
        AssertPositionWithinBounds(localPos);
        Blocks[localPos.X, localPos.Y, localPos.Z] = block;

        Renderer?.BlockUpdated(localPos);
    }

    public BlockData GetBlock(IntVector3 localPos)
    {
        AssertPositionWithinBounds(localPos);
        return Blocks[localPos.X, localPos.Y, localPos.Z];
    }
}