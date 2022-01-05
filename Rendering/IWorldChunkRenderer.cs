using Microsoft.Xna.Framework.Graphics;
using MyGame.Data;

namespace MyGame.Rendering;

public interface IWorldChunkRenderer
{
    void Initialize(GraphicsDevice graphicsDevice);
    void BlockUpdated(IntVector3 localPosition);
    void Draw(GraphicsDevice graphicsDevice);
}