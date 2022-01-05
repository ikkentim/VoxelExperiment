using Microsoft.Xna.Framework;

namespace MyGame.World.Blocks;

public class CobbleBlock : Block
{
    public override TextureReference GetTexture() =>
        new()
        {
            Name = "arrow",
            Uv2 = Vector2.One
        };
}