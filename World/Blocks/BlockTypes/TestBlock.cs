using Microsoft.Xna.Framework;

namespace MyGame.World.Blocks.BlockTypes;

public class TestBlock : BlockBase
{
    public override TextureReference GetTexture() =>
        new()
        {
            Name = "checkered",
            Uv2 = Vector2.One
        };
}