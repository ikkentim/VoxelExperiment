using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MyGame;

public class Game1 : Game
{
    private GraphicsDeviceManager? _graphics;
    private SpriteBatch? _spriteBatch;
    private VertexBuffer? _vertexBuffer;
    private IndexBuffer? _indexBuffer;
    private BasicEffect? _basicEffect;
    private Camera _camera = new Camera();
    private readonly WorldManager _worldManager = new();

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _worldManager.LoadInitialChunks();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _basicEffect = new BasicEffect(GraphicsDevice);


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
            new VertexPositionColor(new Vector3(0, 0, 0), Color.Red),
            new VertexPositionColor(new Vector3(1, 0, 0), Color.Green),
            new VertexPositionColor(new Vector3(0, 0, 1), Color.Blue),
            new VertexPositionColor(new Vector3(1, 0, 1), Color.Red),
            new VertexPositionColor(new Vector3(0, 1, 0), Color.Green),
            new VertexPositionColor(new Vector3(1, 1, 0), Color.Red),
            new VertexPositionColor(new Vector3(0, 1, 1), Color.Blue),
            new VertexPositionColor(new Vector3(1, 1, 1), Color.Red)
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
            3, 7, 5,
        };

        var vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), 8, BufferUsage.WriteOnly);
        var indexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), indices.Length, BufferUsage.WriteOnly);

        vertexBuffer.SetData(vertices);
        indexBuffer.SetData(indices);

        _vertexBuffer = vertexBuffer;
        _indexBuffer = indexBuffer;
    }

    protected override void Update(GameTime gameTime)
    {
        var kb = Keyboard.GetState();

        if (kb.IsKeyDown(Keys.Escape))
            Exit();

        var m = Vector3.Zero;
        var r = Vector2.Zero;
        if (kb.IsKeyDown(Keys.W)) m += Vector3.Forward;
        if (kb.IsKeyDown(Keys.A)) m += Vector3.Left;
        if (kb.IsKeyDown(Keys.S)) m += Vector3.Backward;
        if (kb.IsKeyDown(Keys.D)) m += Vector3.Right;
        if (kb.IsKeyDown(Keys.Q)) m += Vector3.Down;
        if (kb.IsKeyDown(Keys.E)) m += Vector3.Up;

        if (kb.IsKeyDown(Keys.Z)) r += Vector2.UnitY;
        if (kb.IsKeyDown(Keys.X)) r += -Vector2.UnitY;
        if (kb.IsKeyDown(Keys.C)) r += Vector2.UnitX;
        if (kb.IsKeyDown(Keys.V)) r += -Vector2.UnitX;

        m *= (float)gameTime.ElapsedGameTime.TotalSeconds;
        r *= (float)gameTime.ElapsedGameTime.TotalSeconds;

        var q = Quaternion.CreateFromYawPitchRoll(0, r.Y, r.X);

        _camera.Move(m);
        //_camera.Rotate(q);
        HandleMouse(gameTime);

        base.Update(gameTime);
    }

    public static Vector2 MouseRotTemp;
    private void HandleMouse(GameTime gt)
    {
        var mouseDefaultPos = new Vector2(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);
        var mouseSens = 0.5f * (float)gt.ElapsedGameTime.TotalSeconds;

        Vector2 mouseDifference;
        var mouseNow = Mouse.GetState();
        if (mouseNow.X != mouseDefaultPos.X || mouseNow.Y != mouseDefaultPos.Y)
        {
            mouseDifference.X = mouseDefaultPos.X - mouseNow.X;
            mouseDifference.Y = mouseDefaultPos.Y - mouseNow.Y;
            MouseRotTemp.X += mouseSens * mouseDifference.X;
            MouseRotTemp.Y += mouseSens * mouseDifference.Y;

            Mouse.SetPosition((int)mouseDefaultPos.X, (int)mouseDefaultPos.Y);
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _basicEffect!.View = _camera.GetViewMatrix();
        _basicEffect.Projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4, // 90 fov
            Window.ClientBounds.Width / (float)Window.ClientBounds.Height,
            1,
            100);
        _basicEffect.VertexColorEnabled = true;

        GraphicsDevice.SetVertexBuffer(_vertexBuffer);
        GraphicsDevice.Indices = _indexBuffer;

        _worldManager.RenderVisibleChunks(_basicEffect, GraphicsDevice);

        base.Draw(gameTime);
    }
}