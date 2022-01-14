using System;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Data;
using MyGame.World;

namespace MyGame.Rendering;

public interface IChunkRenderer : IDisposable
{
    void Initialize(GraphicsDevice graphicsDevice);
    void BlockUpdated(IntVector3 localPosition, BlockData oldBlock, BlockData newBlock);
    void Draw(GraphicsDevice graphicsDevice);
}