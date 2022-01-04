using System.Drawing;
using Microsoft.Xna.Framework.Graphics;

namespace MyGame.World.Rendering;

public interface IWorldChunkRenderer
{
    void Initialize(GraphicsDevice graphicsDevice);
    void BlockUpdated(IntVector3 localPosition);
    void Draw(GraphicsDevice graphicsDevice);
}