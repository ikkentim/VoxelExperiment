using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace MyGame;

public class Camera
{
    public Transform Transform { get; } = new();

    public Camera()
    {
        Transform.WorldToLocal *= Matrix.CreateTranslation(10, 4, 4);
    }
    
    public Matrix ViewMatrix => Matrix.CreateLookAt(Transform.Position, Transform.Position + Transform.Forward, Transform.Up);
}