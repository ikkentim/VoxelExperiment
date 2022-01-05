using System;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MyGame.Control;
using MyGame.Helpers;
using MyGame.Overlay;
using MyGame.Rendering;
using MyGame.World;
using MyGame.World.Rendering;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace MyGame;

public class Game1 : Game
{
    private readonly Camera _camera = new();
    private readonly WorldManager _worldManager = new();
    private readonly TextureProvider _textureProvider = new();
    private readonly GraphicsDeviceManager _graphics;
    private readonly PlayerController _playerController;
    private WorldRenderer? _worldRenderer;
    private DebuggingLayer? _debuggingLayer;
    private BasicEffect? _testEffect;
    
    private bool _escape;

    public Game1()
    {
        Content.RootDirectory = "Content";

        _graphics = new GraphicsDeviceManager(this);

        IsMouseVisible = true;

        _playerController = new PlayerController(_camera);
    }

    protected override void Initialize()
    {
        GlobalGameContext.Initialize(this);
        
        InitializeDisplay();

        _debuggingLayer = new DebuggingLayer(GraphicsDevice, _camera);
        
        _playerController.StartCaptureMouse();
        
        _worldRenderer = new WorldRenderer(_worldManager, _camera, _textureProvider);
        _worldRenderer.Initialize(GraphicsDevice);
        
        _testEffect = new BasicEffect(GraphicsDevice);

        base.Initialize();
    }

    private void InitializeDisplay()
    {
        _graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width - 400;
        _graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height - 400;
        // _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();

    }

    protected override void LoadContent()
    {
        _debuggingLayer!.LoadContent(Content);

        _textureProvider.LoadContent(Content);

        // Load chunks after render is initialized so chunkrenderers are created.
        _worldManager.LoadInitialChunks();
    }
    
    protected override void Update(GameTime gameTime)
    {
        HandleExitAndMouseCapture();

        _playerController.Update(gameTime.GetDeltaTime());
        _debuggingLayer!.Update(gameTime.GetDeltaTime());

        base.Update(gameTime);
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
                    Exit();
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
            IsActive &&
            m.X >= 0 && m.Y >= 0 && m.X < Window.ClientBounds.Size.X && m.Y < Window.ClientBounds.Size.Y
           )
        {
            _playerController.StartCaptureMouse();
        }
    }


    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        
        _worldRenderer!.Draw(GraphicsDevice);

        DrawTestThings();
        
        // Always draw 2D elements last
        _debuggingLayer!.Draw(gameTime.GetDeltaTime(), GraphicsDevice);

        base.Draw(gameTime);
    }

    #region TestingThings
    
    private void DrawTestThings()
    {

    }
    
    private void DrawLine(Vector3 normal, Color c)
    {
        _testEffect.CurrentTechnique.Passes[0].Apply();
        GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList,
            new[]
            {
                new VertexPositionColor(Vector3.Zero, c),
                new VertexPositionColor(normal, c),
            },
            0,
            2,
            new short[] { 0, 1 }, 0, 1);
   
    }
  
    #endregion
}