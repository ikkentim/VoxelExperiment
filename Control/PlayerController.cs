using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Components;
using MyGame.Platform;
using MyGame.Rendering;
using MyGame.Rendering.Effects;

namespace MyGame.Control;

public class PlayerController
{
    private static readonly float LookPitchLimit = MathHelper.ToRadians(75);
    private readonly Camera _camera;

    private Vector2 _look = new(-MathHelper.PiOver2, 0);
    private Vector3 _position = Vector3.Zero;

    private readonly IRawMouseInput _rawMouseInput;
    private readonly KeyboardInput _keyboardInput = new();

    public PlayerController(Camera camera, VoxelGame game)
    {
        _camera = camera;
        _rawMouseInput = new WindowsRawMouseInput(game);

        _rawMouseInput.Start();
    }

    public bool IsCapturingMouse => _rawMouseInput.IsCapturing;
    
    public void Update(float deltaTime)
    {
        UpdateWalking(deltaTime);
        UpdateLooking(deltaTime);
        
        var matrix =

            Matrix.CreateRotationX(_look.Y) *
            Matrix.CreateRotationY(_look.X) *
            Matrix.CreateTranslation(_position);

        _camera.Transform.WorldToLocal = matrix;
    }

    private void UpdateWalking(float deltaTime)
    {
        const float movementSpeed = 5f;
        
        var kbVec = _keyboardInput.GetArrowsInput();
        kbVec *= deltaTime * movementSpeed;

        var mat = Matrix.CreateRotationX(_look.Y) *
                  Matrix.CreateRotationY(_look.X);

        var delta = Vector3.Transform(kbVec, mat);
        _position += delta;
    }
    
    private void UpdateLooking(float deltaTime)
    {
        const float rotationSpeed = 0.15f;
        
        var input = _rawMouseInput.GetInput();

        var off = input * rotationSpeed;// * deltaTime;

        if (Math.Abs(off.X + off.Y) > 0.2f)
        {

        }

        _look -= input * rotationSpeed * deltaTime;
        _look.X = MathHelper.WrapAngle(_look.X);
        _look.Y = MathHelper.Clamp(_look.Y, -LookPitchLimit, LookPitchLimit);
    }
    
    public void StartCaptureMouse() => _rawMouseInput.Start();
    public void PauseCaptureMouse() => _rawMouseInput.Pause();
    public void ResumeCaptureMouse() => _rawMouseInput.Resume();

    public void StopMouseCapture() => _rawMouseInput.Stop();
}