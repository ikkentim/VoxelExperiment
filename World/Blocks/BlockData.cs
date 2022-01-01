using System.Diagnostics;

namespace MyGame.World.Blocks;

[DebuggerDisplay("{DebugDisplayString,nq}")]
public struct BlockData
{
    internal string DebugDisplayString => $"({Kind?.GetType().Name ?? "None"})";

    public BlockBase? Kind;
    public Face VisibleFaces;
}