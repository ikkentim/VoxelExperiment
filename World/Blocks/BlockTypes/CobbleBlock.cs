using Microsoft.Xna.Framework;

namespace MyGame.World.Blocks.BlockTypes;

public class CobbleBlock : BlockBase
{
    public override TextureReference GetTexture() =>
        new()
        {
            Name = "cobble",
            Uv2 = Vector2.One
        };
}