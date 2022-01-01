using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MyGame.Control;
using MyGame.Helpers;
using MyGame.Overlay;
using MyGame.Rendering;
using MyGame.World;
using MyGame.World.Rendering;

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
        
        // Load chunks after render is initialize so chunkrenderers are created.
        _worldManager.LoadInitialChunks();

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
        
        // Always draw 2D elements last
        _debuggingLayer!.Draw(gameTime.GetDeltaTime(), GraphicsDevice);

        base.Draw(gameTime);
    }

    #region TestingThings

    private Matrix GetTestWorldMatrixForBlockFace(IntVector3 blockPosition, IntVector3 blockFaceNormal)
    {
        var blockFaceNormalAbs = new IntVector3(Math.Abs(blockFaceNormal.X), Math.Abs(blockFaceNormal.Y), Math.Abs(blockFaceNormal.Z));
        
        // around origin of mesh
        var world = Matrix.CreateTranslation(-.5f, -.5f, 0);
        
        // rotate to normal of block face
        if (blockFaceNormal.Z == 0)
        {
            var axis = new Vector3(blockFaceNormal.Y, blockFaceNormal.X, 0);

            world *= Matrix.CreateFromAxisAngle(axis, MathHelper.PiOver2);
        }
        else if (blockFaceNormal.Z == 1)
        {
            world *= Matrix.CreateRotationX(MathHelper.Pi);
        }

        // translate to position
        var tr = (IntVector3.One - blockFaceNormalAbs) * 0.5f;
        world *= Matrix.CreateTranslation(tr);
        world *= Matrix.CreateTranslation(blockPosition);
        
        if (blockFaceNormal == blockFaceNormalAbs)
        {
            world *= Matrix.CreateTranslation(blockFaceNormal);
        }

        return world;
    }

    private void DrawTestFace(Matrix world)
    {
        _testEffect!.View = _camera.ViewMatrix;
        _testEffect.Projection = GlobalGameContext.Current.Projection;
        _testEffect.World = world;
        
        foreach (var pass in _testEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineStrip, FaceVerticesLine, 0, FaceVerticesLine.Length, FaceIndicesLine, 0, 5);
        }
    }
    private void DrawTestThings()
    {
        DrawTestFace(GetTestWorldMatrixForBlockFace(IntVector3.One, IntVector3.UnitX));
        DrawTestFace(GetTestWorldMatrixForBlockFace(IntVector3.One, -IntVector3.UnitX));
        DrawTestFace(GetTestWorldMatrixForBlockFace(IntVector3.One, IntVector3.UnitY));
        DrawTestFace(GetTestWorldMatrixForBlockFace(IntVector3.One, -IntVector3.UnitY));
        DrawTestFace(GetTestWorldMatrixForBlockFace(IntVector3.One, IntVector3.UnitZ));
        DrawTestFace(GetTestWorldMatrixForBlockFace(IntVector3.One, -IntVector3.UnitZ));
    }

    private static readonly VertexPosition[] FaceVerticesLine = {
        new(new Vector3(0, 0, 0)),
        new(new Vector3(1, 0, 0)),
        new(new Vector3(0, 1, 0)),
        new(new Vector3(1, 1, 0))
    };

    private static readonly short[] FaceIndicesLine = {
        2, 0, 1, 3, 2, 1
    };

    #endregion
}