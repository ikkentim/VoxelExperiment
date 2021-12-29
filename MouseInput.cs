using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MyGame;

public class MouseInput
{
    private bool _isCapturing;
    private MouseState _lastMouseState;

    // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
    public bool IsCapturing => _isCapturing;

    public void StartCapture()
    {
        if (_isCapturing)
        {
            return;
        }

        _isCapturing = true;

        GlobalGameContext.Current.Game.IsMouseVisible = false;
        
        var center = GlobalGameContext.Current.WindowSize / 2;
        Mouse.SetPosition((int)center.X, (int)center.Y);
        _lastMouseState = Mouse.GetState();
    }

    public void StopCapture()
    {
        _isCapturing = false;
        GlobalGameContext.Current.Game.IsMouseVisible = true;
    }

    public Vector2 GetInput()
    {
        if (!_isCapturing)
        {
            return Vector2.Zero;
        }

        var state = Mouse.GetState();
        
        var position = state.Position.ToVector2();
        var lastPosition = _lastMouseState.Position.ToVector2();
        
        // Only recenter mouse when close to window borders to avoid setting the mouse position at a high rate.

        var windowSize = GlobalGameContext.Current.WindowSize;

        var left = windowSize * 0.1f;
        var right = windowSize * 0.9f;

        if (position.X < left.X || position.Y < left.Y || position.X > right.X || position.Y > right.Y)
        {
            var center = GlobalGameContext.Current.WindowSize / 2;
            Mouse.SetPosition((int)center.X, (int)center.Y);
            state = Mouse.GetState();
        }

        _lastMouseState = state;
        return position - lastPosition;
    }
}