using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MyGame;

public class KeyboardInput
{
    public Vector3 GetArrowsInput()
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
}