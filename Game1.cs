using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MyGame;

public class Game1 : Game
{
    private Texture2D _testTexture;
    private readonly Camera _camera = new();
    private readonly GraphicsDeviceManager _graphics;
    private readonly PlayerController _playerController;
    private readonly WorldManager _worldManager = new();
    private BasicEffect? _basicEffect;
    private IndexBuffer? _dbgindexBuffer;
    private VertexBuffer? _dbgvertexBuffer;

    private VertexPositionColor[]? _dbgVertices;
    private BasicEffect? _debugEffect;
    private bool _escape;
    private IndexBuffer? _indexBuffer;
    private SpriteBatch? _spriteBatch;
    private VertexBuffer? _vertexBuffer;

    public Game1()
    {
        Content.RootDirectory = "Content";

        _graphics = new GraphicsDeviceManager(this);

        IsMouseVisible = true;

        _playerController = new PlayerController(_camera);
    }

    protected override void Initialize()
    {
        _worldManager.LoadInitialChunks();

        GlobalGameContext.Initialize(this);

        _playerController.StartCaptureMouse();

        _graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width - 400;
        _graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height - 400;
        // _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _basicEffect = new BasicEffect(GraphicsDevice);
        _debugEffect = new BasicEffect(GraphicsDevice);

        _testTexture = Content.Load<Texture2D>("checkered");
        _basicEffect.Texture = _testTexture;
        _basicEffect.TextureEnabled = true;

        /*    6 ___________7
         *     /|         /|
         *  4 / |        / |
         * y+/_________ /5 |
         *  |   |      |   |
         *  |  2|______|__ |3
         *  |  /z+     |  /
         *  | /        | /
         *  |/_________|/
         *  0       x+ 1
         */

        var vertices = new[]
        {
            new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(1, 0, 0), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(0, 0, 1), new Vector2(0, 1)),
            new VertexPositionTexture(new Vector3(1, 0, 1), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3(0, 1, 0), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(0, 1, 1), new Vector2(0, 1)),
            new VertexPositionTexture(new Vector3(1, 1, 1), new Vector2(1, 1)),
        };

        var indices = new short[]
        {
            2, 1, 0, // bottom
            2, 3, 1,
            4, 5, 6, // top
            5, 7, 6,
            0, 1, 4, // front
            1, 5, 4,
            3, 2, 7, // back
            2, 6, 7,
            2, 0, 6, // left
            0, 4, 6,
            1, 3, 5, // right
            3, 7, 5
        };

        _vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), 8, BufferUsage.WriteOnly);
        _indexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), indices.Length, BufferUsage.WriteOnly);
        
        _vertexBuffer.SetData(vertices);
        _indexBuffer.SetData(indices);

        InitializeDebugging();
    }

    private void InitializeDebugging()
    {
        _dbgVertices = new[]
        {
            new VertexPositionColor(Vector3.Zero, Color.Red),
            new VertexPositionColor(Vector3.UnitX * 5, Color.Red),
            new VertexPositionColor(Vector3.Zero, Color.Green),
            new VertexPositionColor(Vector3.UnitY * 5, Color.Green),
            new VertexPositionColor(Vector3.Zero, Color.Blue),
            new VertexPositionColor(Vector3.UnitZ * 5, Color.Blue)
        };
        var dbgIndices = new short[] { 0, 1, 2, 3, 4, 5 };
        _dbgvertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), 6, BufferUsage.WriteOnly);
        _dbgindexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), 6, BufferUsage.WriteOnly);

        _dbgvertexBuffer.SetData(_dbgVertices);
        _dbgindexBuffer.SetData(dbgIndices);
    }

    protected override void Update(GameTime gameTime)
    {
        HandleExitAndMouseCapture();

        _playerController.Update(gameTime.GetDeltaTime());

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

        _basicEffect!.View = _camera.ViewMatrix;
        _basicEffect.Projection = GlobalGameContext.Current.Projection;
        
        GraphicsDevice.SetVertexBuffer(_vertexBuffer);
        GraphicsDevice.Indices = _indexBuffer;

        _worldManager.RenderVisibleChunks(_basicEffect, _camera, GraphicsDevice);

        DebugDraw();

        base.Draw(gameTime);
    }

    private void DebugDraw()
    {
        // axis lines in world space
        _debugEffect!.View = _camera.ViewMatrix;
        _debugEffect.Projection = GlobalGameContext.Current.Projection;
        _debugEffect.World = Matrix.Identity;
        _debugEffect.VertexColorEnabled = true;
        _debugEffect.CurrentTechnique.Passes[0].Apply();

        GraphicsDevice.SetVertexBuffer(_dbgvertexBuffer);
        GraphicsDevice.Indices = _dbgindexBuffer;
        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, 3);

        // axis lines in cam space

        var cam = _camera.Transform.Backward * 50;
        var up = _camera.Transform.Up;

        _debugEffect.View = Matrix.CreateLookAt(cam, Vector3.Zero, up);
        _debugEffect.CurrentTechnique.Passes[0].Apply();

        GraphicsDevice.SetVertexBuffer(_dbgvertexBuffer);
        GraphicsDevice.Indices = _dbgindexBuffer;
        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, 3);
    }
}