using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MyGame.Control;
using MyGame.Extensions;

namespace MyGame.Components;

public class PlayerControllerComponent : GameComponent
{
    private bool _escape;
    private readonly PlayerController _playerController;

    public PlayerControllerComponent(VoxelGame game) : base(game)
    {
        _playerController = new PlayerController(game.Camera);
    }

    private new VoxelGame Game => (VoxelGame)base.Game;

    public override void Initialize()
    {
        _playerController.StartCaptureMouse();
    }

    public override void Update(GameTime gameTime)
    {
        _playerController.Update(gameTime.GetDeltaTime());

        HandleExitAndMouseCapture();
    }
    
    private void HandleExitAndMouseCapture()
    {
        var kb = Keyboard.GetState();

        if (kb.IsKeyDown(Keys.Escape))
        {
            if (!_escape)
            {
                if (_playerController.IsCapturingMouse)
                {
                    _playerController.StopMouseCapture();
                }
                else
                {
                    Game.Exit();
                }
            }

            _escape = true;
        }
        else
        {
            _escape = false;
        }

        var m = Mouse.GetState();
        if (m.LeftButton == ButtonState.Pressed &&
            !_playerController.IsCapturingMouse &&
            Game.IsActive &&
            m.X >= 0 && m.Y >= 0 && m.X < Game.Window.ClientBounds.Size.X && m.Y < Game.Window.ClientBounds.Size.Y
           )
        {
            _playerController.StartCaptureMouse();
        }
    }
}