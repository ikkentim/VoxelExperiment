using Microsoft.Xna.Framework;
using MyGame.Data;
using MyGame.World;

namespace MyGame.Extensions;

public struct RayHitInfo
{
    public static readonly RayHitInfo Empty = new();

    public RayHitInfo(bool isHit, float distance, BlockData block, IntVector3 position, Vector3 hitPosition, BlockFace face)
    {
        IsHit = isHit;
        Distance = distance;
        Block = block;
        Position = position;
        HitPosition = hitPosition;
        Face = face;
    }

    public bool IsHit { get; }
    public float Distance { get; }
    public BlockData Block { get; }
    public IntVector3 Position { get; }
    public Vector3 HitPosition { get; }
    public BlockFace Face { get; }
}