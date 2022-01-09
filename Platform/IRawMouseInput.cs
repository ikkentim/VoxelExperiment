using Microsoft.Xna.Framework;

namespace MyGame.Platform;

public interface IRawMouseInput
{
    bool IsCapturing { get; }
    void Start();
    void Stop();
    Vector2 GetInput();
    void Pause();
    void Resume();
}