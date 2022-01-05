using Microsoft.Xna.Framework;

namespace MyGame.Extensions;

public static class GameTimeExtensions
{
    public static float GetDeltaTime(this GameTime gameTime) => (float)gameTime.ElapsedGameTime.TotalSeconds;
}