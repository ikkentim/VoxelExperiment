using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MyGame.Control;
using MyGame.Extensions;

namespace MyGame.Components;

public class PlayerControllerComponent : DrawableGameComponent
{
    private bool _escape;
    private readonly PlayerController _playerController;
    private RayHitInfo _lookingAtBlock;

    public PlayerControllerComponent(VoxelGame game) : base(game)
    {
        _playerController = new PlayerController(game.Camera, game);
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

        HandleLookingAtBlock();
    }

    private void HandleLookingAtBlock()
    {
        _lookingAtBlock = Game.Camera.Transform.Ray.Cast(100f, Game.WorldManager);
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
                    _playerController.PauseCaptureMouse();
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
            _playerController.ResumeCaptureMouse();
        }
    }

    public override void Draw(GameTime gameTime)
    {
        if (_lookingAtBlock.IsHit)
        {
            Game.BlockOutlineRenderer.Render(_lookingAtBlock.Position, _lookingAtBlock.Block);
        }

        base.Draw(gameTime);
    }
}