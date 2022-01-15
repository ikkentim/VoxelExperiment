using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Extensions;

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

    protected override void LoadContent()
    {
        _font = Game.AssetManager.GetDebugFont();
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
            _debugEffect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.SetVertexBuffer(_vertexBuffer);
            GraphicsDevice.Indices = _indexBuffer;
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, 3);
        }

        // axis lines in cam space
        if (DrawCameraAxis)
        {
            _debugEffect.View = Matrix.CreateLookAt(Game.Camera.Transform.Backward * 0.2f, Vector3.Zero, Game.Camera.Transform.Up);
            _debugEffect.World = Matrix.CreateScale(0.01f);
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
}