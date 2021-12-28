using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MyGame;

public class PlayerController
{
    private readonly Camera _camera;
    private bool _isCapturingMouse = false;

    public PlayerController(Camera camera)
    {
        _camera = camera;
    }

    public bool IsCapturingMouse => _isCapturingMouse;

    public void Update(float deltaTime)
    {
        UpdateWalking(deltaTime);
        UpdateLooking(deltaTime);
    }

    private void UpdateWalking(float deltaTime)
    {
        const float movementSpeed = 10f;

        var kbVec = GetKeyboardInput();
        kbVec *= deltaTime * movementSpeed;

        var delta = Vector3.Transform(kbVec, _camera.Transform.Rotation);
        _camera.Transform.WorldToLocal *= Matrix.CreateTranslation(delta);
        //_camera.Position += delta;

        Debug.WriteLine(_camera.Position);
    }

    private float _yaw;
    private float _pitch = MathHelper.PiOver2;
        
    private void UpdateLooking(float deltaTime)
    {
        const float rotationSpeed = 0.05f;
        
        var (yaw, pitch) = MouseOffset * rotationSpeed;

        //_yaw += yaw;
        //_pitch += pitch;
        //_pitch = MathHelper.Clamp(_pitch, -MathHelper.PiOver2, MathHelper.PiOver2);

        var mod = Matrix.CreateFromAxisAngle(Vector3.Up, yaw) * Matrix.CreateFromAxisAngle(Vector3.Right, pitch);

        _camera.Transform.WorldToLocal *= mod;
        CenterMouse();
    }

    public void StartCaptureMouse()
    {
        if (_isCapturingMouse)
        {
            return;
        }

        _isCapturingMouse = true;

        GlobalGameContext.Current.Game.IsMouseVisible = false;

        CenterMouse();
    }

    public void StopMouseCapture()
    {
        if (!_isCapturingMouse)
        {
            return;
        }

        _isCapturingMouse = false;
        GlobalGameContext.Current.Game.IsMouseVisible = true;
    }
        
    private Vector3 GetKeyboardInput()
    {
        var kbVec = Vector3.Zero;
            
        var kb = Keyboard.GetState();
        if (kb.IsKeyDown(Keys.W)) kbVec += Vector3.Forward;
        if (kb.IsKeyDown(Keys.A)) kbVec += Vector3.Left;
        if (kb.IsKeyDown(Keys.S)) kbVec += Vector3.Backward;
        if (kb.IsKeyDown(Keys.D)) kbVec += Vector3.Right;
        if (kb.IsKeyDown(Keys.Q)) kbVec += Vector3.Down;
        if (kb.IsKeyDown(Keys.E)) kbVec += Vector3.Up;

        return kbVec;
    }

    private Vector2 MouseCenter => GlobalGameContext.Current.WindowSize / 2;
    private Vector2 MousePosition => Mouse.GetState().Position.ToVector2();

    private Vector2 MouseOffset => _isCapturingMouse ? MousePosition - MouseCenter : Vector2.Zero;

    private void CenterMouse()
    {
        if (!_isCapturingMouse)
        {
            return;
        }

        var centerMouse = MouseCenter;
        Mouse.SetPosition((int)centerMouse.X, (int)centerMouse.Y);
    }

}