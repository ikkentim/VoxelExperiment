using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MyGame.Control;
using MyGame.Extensions;
using MyGame.World;
using MyGame.World.Blocks;

namespace MyGame.Components;

public class PlayerControllerComponent : DrawableGameComponent
{
    private readonly PlayerController _playerController;
    private bool _escape;
    private RayHitInfo _lookingAtBlock;
    private int _selectedBlock;

    public PlayerControllerComponent(VoxelGame game) : base(game)
    {
        _playerController = new PlayerController(game.Camera, game);
    }

    private new VoxelGame Game => (VoxelGame)base.Game;

    public override void Initialize()
    {
        _playerController.StartCaptureMouse();
    }

    private int _scrollWheel;

    public override void Update(GameTime gameTime)
    {
        _playerController.Update(gameTime.GetDeltaTime());

        HandleExitAndMouseCapture();
        HandleLookingAtBlock();

        var newScroll = Mouse.GetState().ScrollWheelValue;
        
        if (newScroll > _scrollWheel)
        {
            _selectedBlock++;
        }
        else if (newScroll < _scrollWheel)
        {
            _selectedBlock--;
        }
        _scrollWheel = newScroll;
        if (_lookingAtBlock.IsHit)
        {
            if (_playerController.PlaceBlock)
            {
                // bad temporary code
                var list = Game.BlockRegistry.GetBlockTypes().Where(x => x is not AirBlock).ToList();
                var blockIndex = Math.Abs(_selectedBlock) % list.Count;

                Game.WorldManager.SetBlock(_lookingAtBlock.Position + BlockFaces.GetNormal(_lookingAtBlock.Face), new BlockState
                {
                    BlockType = list[blockIndex]
                });
            }
            else if (_playerController.RemoveBlock)
            {
                Game.WorldManager.SetBlock(_lookingAtBlock.Position, default);
            }
        }
    }

    private void HandleLookingAtBlock()
    {
        _lookingAtBlock = Game.Camera.Transform.Ray.Cast(20f, Game.WorldManager);
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