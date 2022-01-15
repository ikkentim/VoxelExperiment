using Microsoft.Xna.Framework;
using MyGame.Data;

namespace MyGame.Rendering;

public class Camera
{
    public Camera()
    {
        // default camera position
        // todo: is currently overwritten...
        Transform.WorldToLocal *= Matrix.CreateTranslation(10, 4, 4);
    }

    public Transform Transform { get; } = new();

    public Matrix ViewMatrix => Matrix.CreateLookAt(Transform.Position, Transform.Position + Transform.Forward, Transform.Up);
}