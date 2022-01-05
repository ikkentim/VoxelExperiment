using System.Diagnostics;

namespace MyGame.World;

[DebuggerDisplay("{DebugDisplayString,nq}")]
public struct BlockData
{
    internal string DebugDisplayString => $"({Kind?.GetType().Name ?? "None"})";

    public Block? Kind;
    public BlockFace VisibleBlockFaces;
}
