using Microsoft.Xna.Framework.Graphics;

namespace MyGame.World.Rendering;

public interface IWorldChunkRenderer
{
    void Initialize();
    void BlockUpdated(IntVector3 localPosition);
    void Draw(GraphicsDevice graphicsDevice);
}