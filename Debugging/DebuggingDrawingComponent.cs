using System.Drawing.Drawing2D;
using System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MyGame.Extensions;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace MyGame.Debugging;

public class DebuggingDrawingComponent : DrawableGameComponent
{
    private const bool DrawWorldAxis = true;
    private const bool DrawCameraAxis = true;

    private readonly BasicEffect _debugEffect;
    private readonly IndexBuffer _indexBuffer;
    private readonly SpriteBatch _spriteBatch;
    private readonly VertexBuffer _vertexBuffer;
    private SpriteFont? _font;
    private float _time;

    public DebuggingDrawingComponent(Game game) : base(game)
    {
        var vertices = new[]
        {
            new VertexPositionColor(Vector3.Zero, Color.Red),
            new VertexPositionColor(Vector3.UnitX, Color.Red),
            new VertexPositionColor(Vector3.Zero, Color.Green),
            new VertexPositionColor(Vector3.UnitY, Color.Green),
            new VertexPositionColor(Vector3.Zero, Color.Blue),
            new VertexPositionColor(Vector3.UnitZ, Color.Blue)
        };
        var indices = new short[] { 0, 1, 2, 3, 4, 5 };

        _vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), 6, BufferUsage.WriteOnly);
        _indexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), 6, BufferUsage.WriteOnly);

        _vertexBuffer.SetData(vertices);
        _indexBuffer.SetData(indices);

        _debugEffect = new BasicEffect(GraphicsDevice);
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    private new VoxelGame Game => (VoxelGame)base.Game;

    public override void Initialize()
    {
        DrawOrder = 1000;
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _font = Game.AssetManager.GetDebugFont();
    }

    private KeyboardState _keyboardState;

    private Vector3[]? _corners;
    public override void Update(GameTime gameTime)
    {
        var kb = Keyboard.GetState();

        if (kb.IsKeyDown(Keys.F8) && _keyboardState.IsKeyUp(Keys.F8))
        {
            // capture view frustum
            var view = Game.Camera.ViewMatrix;

            var corners = new[]
            {
                new Vector4(-1.0f, -1.0f, 1.0f, 1.0f), 
                new Vector4(-1.0f, -1.0f, 0, 1.0f),
                new Vector4(-1.0f, 1.0f, 1.0f, 1.0f),
                new Vector4(-1.0f, 1.0f, 0, 1.0f),
                new Vector4(1.0f, -1.0f, 1.0f, 1.0f),
                new Vector4(1.0f, -1.0f, 0, 1.0f), 
                new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                new Vector4(1.0f, 1.0f, 0, 1.0f)
            };
            var inverseVpMatrix = Matrix.Invert(view * GlobalGameContext.Current.Projection);

            var corners2 = new Vector4[8];
            Vector4.Transform(corners, ref inverseVpMatrix, corners2);
            _corners = new Vector3[8];
            for (var i = 0; i < 8; i++)
            {
                var tra = Vector4.Transform(corners[i], inverseVpMatrix);
                tra /= tra.W;
                _corners[i] = tra.ToXyz();
            }

        }
        _keyboardState = kb;

        base.Update(gameTime);
    }
    
    public override void Draw(GameTime gameTime) => Draw(gameTime.GetDeltaTime());

    public void Draw(float deltaTime)
    {
        _debugEffect.Projection = GlobalGameContext.Current.Projection;

        // axis lines in world space
        if (DrawWorldAxis)
        {
            _debugEffect.View = Game.Camera.ViewMatrix;
            _debugEffect.World = Matrix.Identity;
            _debugEffect.VertexColorEnabled = true;
            _debugEffect.DiffuseColor = Vector3.One;
            _debugEffect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.SetVertexBuffer(_vertexBuffer);
            GraphicsDevice.Indices = _indexBuffer;
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, 3);
        }
        
        // view frustum
        if (_corners != null)
        {
            _debugEffect.DiffuseColor = Color.Red.ToVector3();
            _debugEffect.VertexColorEnabled = false;
            _debugEffect.World = Matrix.Identity;

            for (var i = 0; i < _corners.Length - 1; i += 2)
            {
                DrawLine(_corners[i], _corners[i + 1]);
            }
            
            DrawLine(_corners[0], _corners[2]);
            DrawLine(_corners[0], _corners[4]);
            DrawLine(_corners[2], _corners[6]);
            DrawLine(_corners[4], _corners[6]);
            
            DrawLine(_corners[1], _corners[3]);
            DrawLine(_corners[1], _corners[5]);
            DrawLine(_corners[3], _corners[7]);
            DrawLine(_corners[5], _corners[7]);
        }

        // axis lines in cam space
        if (DrawCameraAxis)
        {
            _debugEffect.View = Matrix.CreateLookAt(Game.Camera.Transform.Backward * 0.2f, Vector3.Zero, Game.Camera.Transform.Up);
            _debugEffect.World = Matrix.CreateScale(0.01f);
            _debugEffect.DiffuseColor = Vector3.One;
            _debugEffect.VertexColorEnabled = true;
            _debugEffect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.SetVertexBuffer(_vertexBuffer);
            GraphicsDevice.Indices = _indexBuffer;
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, 3);
        }

        // draw fps
        _time += (deltaTime - _time) / 5;
        _spriteBatch.Begin();
        _spriteBatch.DrawString(_font, $"FPS: {1 / _time}", new Vector2(10, 10), Color.White, 0, Vector2.Zero, Vector2.One,
            SpriteEffects.None, 0);


        _spriteBatch.DrawString(_font,
            PerformanceCounters.Drawing.GetText() + PerformanceCounters.Update.GetText() + PerformanceCounters.Cumulative.GetText(),
            new Vector2(10, 50), Color.White, 0, Vector2.Zero, Vector2.One,
            SpriteEffects.None, 0);
        _spriteBatch.End();

        PerformanceCounters.Drawing.Reset();

        // reset DepthStencil state after drawing 2d
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
    }

    private void DrawLine(Vector3 a, Vector3 b)
    {
        _debugEffect.CurrentTechnique.Passes[0].Apply();
        GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, new[] { new VertexPosition(a), new VertexPosition(b) }, 0, 2,
            new short[] { 0, 1 }, 0, 1);
    }
}