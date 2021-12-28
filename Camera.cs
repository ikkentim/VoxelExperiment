using Microsoft.Xna.Framework;

namespace MyGame;

public class Camera
{
    public Transform Transform { get; } = new();

    public Camera()
    {
        Transform.WorldToLocal *= Matrix.CreateTranslation(10, 4, 4);
    }

    public Vector3 Position
    {
        get => Transform.Position;
    }

    public Quaternion Rotation => Transform.Rotation;

    public Matrix ViewMatrix => Matrix.CreateLookAt(Position, Position + Transform.Forward, Transform.Up);
}