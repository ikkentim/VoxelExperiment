using Microsoft.Xna.Framework;
using MyGame.Data;
using MyGame.World;
using MyGame.World.Blocks;

namespace MyGame.Components;

public static class RayExtensions
{
    public static RayHitInfo Cast(this Ray ray, float radius, WorldManager world)
    {
        // http://www.cse.yorku.ca/~amana/research/grid.pdf
        
        var (origin, direction) = ray;
        direction.Normalize();
        
        var voxel = VectorHelper.Floor(origin);

        var step = new IntVector3(
            direction.X < 0 ? -1 : direction.X > 0 ? 1 : 0,
            direction.Y < 0 ? -1 : direction.Y > 0 ? 1 : 0,
            direction.Z < 0 ? -1 : direction.Z > 0 ? 1 : 0
        );

        var tDelta = Vector3.One / VectorHelper.Abs(direction);

        var dist = new Vector3(
            step.X > 0 ? voxel.X + 1 - origin.X : origin.X - voxel.X,
            step.Y > 0 ? voxel.Y + 1 - origin.Y : origin.Y - voxel.Y,
            step.Z > 0 ? voxel.Z + 1 - origin.Z : origin.Z - voxel.Z
        );

        var (maxX, maxY, maxZ) = tDelta * dist;
        float distance;
        do
        {
            BlockFace face;
            if (maxX < maxY)
            {
                if (maxX < maxZ)
                {
                    distance = maxX;
                    voxel.X += step.X;
                    maxX += tDelta.X;
                    face = step.X < 0 ? BlockFace.East : BlockFace.West;
                }
                else
                {
                    distance = maxZ;
                    voxel.Z += step.Z;
                    maxZ += tDelta.Z;
                    face = step.Z < 0 ? BlockFace.South : BlockFace.North;
                }
            }
            else
            {
                if (maxY < maxZ)
                {
                    distance = maxY;
                    voxel.Y += step.Y;
                    maxY += tDelta.Y;
                    face = step.Y < 0 ? BlockFace.Top : BlockFace.Bottom;
                }
                else
                {
                    distance = maxZ;
                    voxel.Z += step.Z;
                    maxZ += tDelta.Z;
                    face = step.Z < 0 ? BlockFace.South : BlockFace.North;
                }
            }

            var block = world.GetBlock(voxel);
            if (block.Kind is not AirBlock)
            {
                var hitPosition = origin + direction * distance;
                return new RayHitInfo(true, distance, block, voxel, hitPosition, face);
            }
        } while (distance < radius);

        return RayHitInfo.Empty;
    }
}