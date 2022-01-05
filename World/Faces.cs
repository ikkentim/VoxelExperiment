using System;

namespace MyGame.World;

public static class Faces
{
    public const Face PositiveFaces = Face.Top | Face.East | Face.South;
    public const Face NegativeFaces = Face.Bottom | Face.West | Face.North;

    public static IntVector3[] Normals { get; } =
    {
        IntVector3.UnitX,
        IntVector3.UnitY,
        IntVector3.UnitZ,
        -IntVector3.UnitX,
        -IntVector3.UnitY,
        -IntVector3.UnitZ,
    };

    public static Face[] AllFaces { get; } =
    {
        Face.East,
        Face.Top,
        Face.South,
        Face.West,
        Face.Bottom,
        Face.North
    };
    
    public static Face GetOpposite(Face face)
    {
        if ((face & PositiveFaces) != Face.None)
            return (Face)((int)face << 1);

        return (Face)((int)face >> 1);
    }
    
    public static bool IsPositive(Face face) => Face.None != (PositiveFaces & face);
    public static bool IsNegative(Face face) => Face.None != (NegativeFaces & face);

    public static IntVector3 GetNormal(Face face)
    {
        return face switch
        {
            Face.Bottom => -IntVector3.UnitY,
            Face.Top => IntVector3.UnitY,
            Face.South => IntVector3.UnitZ,
            Face.North => -IntVector3.UnitZ,
            Face.West => -IntVector3.UnitX,
            Face.East => IntVector3.UnitX,
            _ => throw new ArgumentOutOfRangeException(nameof(face), face, null)
        };
    }

    public static IntVector3 GetUp(Face face)
    {
        return face switch
        {
            Face.Bottom => IntVector3.UnitZ,
            Face.Top => IntVector3.UnitZ,
            Face.South => IntVector3.UnitY,
            Face.North => IntVector3.UnitY,
            Face.West => IntVector3.UnitY,
            Face.East => IntVector3.UnitY,
            _ => throw new ArgumentOutOfRangeException(nameof(face), face, null)
        };
    }
}