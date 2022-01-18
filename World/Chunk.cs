using System;
using MyGame.Data;
using MyGame.World.Blocks;

namespace MyGame.World;

public class Chunk : IDisposable
{
    public const int Size = 16;

    private readonly PaletteBackedArray<BlockState> _blockStates = new(Size * Size * Size);

    public WorldManager World { get; }
    public IntVector3 ChunkPosition { get; }
    public IntVector3 WorldPosition => ChunkPosition * Size;
    public int BlockCount { get; private set; }

    public Chunk(WorldManager world, IntVector3 chunkPosition)
    {
        World = world;
        ChunkPosition = chunkPosition;

    }

    public void OnLoaded() { }

    public void SetBlock(IntVector3 localPos, BlockState state)
    {
        if (state.BlockType is AirBlock)
        {
            // make type of air blocks null to make state equal the default BlockState value
            // the palette of the blockStates array has a special case where a default value
            // in a palette is always found at index 0
            state.BlockType = null;
        }

        var oldState = _blockStates.Replace(GetIndex(localPos), state);

        if (oldState == state)
        {
            return;
        }

        var oldFilled = oldState.BlockType != null;
        var newFilled = state.BlockType != null;

        if (newFilled && !oldFilled)
        {
            BlockCount++;
        }
        else if (oldFilled && !newFilled)
        {
            BlockCount--;
        }

        GlobalGameContext.Current.Game.WorldRender.MarkChunkDirty(this); // todo; event based or something..
    }
    
    private static int GetIndex(IntVector3 localPos)
    {
        return localPos.X * Size * Size + localPos.Y * Size + localPos.Z;
    }
    
    public BlockState GetBlock(IntVector3 localPos)
    {
        var state = _blockStates.Get(GetIndex(localPos));
        state.BlockType ??= AirBlock.Instance;
        return state;
    }

    public BlockState GetRelativeBlock(IntVector3 localPos)
    {
        static void OffsetPosToChunk(ref int pos, ref int chunk)
        {
            switch (pos)
            {
                case < 0:
                    pos += Size;
                    chunk--;
                    break;
                case >= Size:
                    pos -= Size;
                    chunk++;
                    break;
            }
        }

        var chunkPos = IntVector3.Zero;

        OffsetPosToChunk(ref localPos.X, ref chunkPos.X);
        OffsetPosToChunk(ref localPos.Y, ref chunkPos.Y);
        OffsetPosToChunk(ref localPos.Z, ref chunkPos.Z);

        if (chunkPos == IntVector3.Zero)
        {
            return GetBlock(localPos);
        }

        var chunk = World.GetChunk(chunkPos);
        var block = chunk?.GetBlock(localPos) ?? default;

        block.BlockType ??= AirBlock.Instance; // set to air if chunk is not yet loaded.

        return block;
    }

    public void Dispose()
    {
        _blockStates.Dispose();
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
}