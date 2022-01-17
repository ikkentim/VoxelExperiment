using System;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Data;
using MyGame.World;

namespace MyGame.Rendering;

public interface IChunkRenderer : IDisposable
{
    void Initialize(GraphicsDevice graphicsDevice);
    void BlockUpdated(IntVector3 localPosition, BlockState oldBlock, BlockState newBlock);
    void Draw(GraphicsDevice graphicsDevice);
}