using System;
using Microsoft.Xna.Framework;

namespace MyGame.World;

public static class Block
{
    public static Vector3 GetLocalFacePosition(IntVector3 localPos, Face face)
    {
        var normal = Faces.GetNormal(face);
        var up = Faces.GetUp(face);

        if (Faces.IsPositive(face))
        {
            localPos += normal;
        }

        var cross = Vector3.Cross(normal, up);
        var absCross = new Vector3(
            MathF.Abs(cross.X),
            MathF.Abs(cross.Y),
            MathF.Abs(cross.Z)
        );

        return localPos + (absCross + up) * 0.5f;
    }
}