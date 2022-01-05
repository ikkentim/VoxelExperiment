using Microsoft.Xna.Framework;

namespace MyGame.World.Blocks;

public class TestBlock : Block
{
    public override TextureReference GetTexture() =>
        new()
        {
            Name = "checkered",
            Uv2 = Vector2.One
        };
}