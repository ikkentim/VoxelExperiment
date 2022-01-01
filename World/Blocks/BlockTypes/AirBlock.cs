namespace MyGame.World.Blocks.BlockTypes;

public class AirBlock : BlockBase
{
    public static AirBlock Instance { get; } = new();
    public override TextureReference GetTexture() => new TextureReference();
}