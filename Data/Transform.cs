using Microsoft.Xna.Framework;
using MyGame.Extensions;

namespace MyGame.Data;

public class Transform
{
    private Matrix _matrix = Matrix.Identity;
    public Vector3 Position => _matrix.Translation;
    public Quaternion Rotation => Quaternion.CreateFromRotationMatrix(_matrix);
    public Vector3 Forward => Vector3.Transform(Vector3.Forward, Rotation).Normalized();
    public Vector3 Left => Vector3.Transform(Vector3.Left, Rotation).Normalized();
    public Vector3 Right => Vector3.Transform(Vector3.Right, Rotation).Normalized();
    public Vector3 Backward => Vector3.Transform(Vector3.Backward, Rotation).Normalized();
    public Vector3 Up => Vector3.Transform(Vector3.Up, Rotation).Normalized();
    public Vector3 Down => Vector3.Transform(Vector3.Down, Rotation).Normalized();

    public Matrix WorldToLocal
    {
        get => _matrix;
        set => _matrix = value;
    }
    public Matrix LocalToWorld => Matrix.Invert(WorldToLocal);
    
}