using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace MyGame;

public class Camera
{
    public Vector3 Position { get; set; } = new(10, 4, 4);
    public Quaternion Rotation { get; set; }

    public void Move(Vector3 movement)
    {
        if (movement != Vector3.Zero)
        {
            Position += Vector3.Transform(movement, Rotation);

            Debug.WriteLine("cam pos: " + Position);
        }

    }
    public void Rotate(Quaternion rot)
    {
        if (rot != Quaternion.Identity)
        {
            Rotation += rot;

            Debug.WriteLine("cam rot: " + Rotation);
        }
    }
    
    private Matrix UpdateViewMatrix()
    {
        var updownRot = Game1.MouseRotTemp.Y;
        var leftrightRot = Game1.MouseRotTemp.X;


        var cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);
        
        var target = Position + Vector3.Transform(Vector3.Forward, cameraRotation);
        var up = Vector3.Transform(Vector3.Up, cameraRotation);

        Debug.WriteLine($"Matrix.CreateLookAt({Position}, {target}, {up})");
        return Matrix.CreateLookAt(Position, target, up);
    }

    public Matrix GetViewMatrix()
    {
        return UpdateViewMatrix();

        var target = Vector3.Transform(Vector3.Forward, Rotation) + Position;

        //target = Vector3.One * 5;//fixed for now...
        return Matrix.CreateLookAt(Position, target, Vector3.Up);
    }
}