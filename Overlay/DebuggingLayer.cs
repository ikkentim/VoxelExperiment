using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Rendering;

namespace MyGame.Overlay;

public class DebuggingLayer
{
    private readonly Camera _camera;
    private readonly BasicEffect _debugEffect;
    private readonly IndexBuffer _indexBuffer;
    private readonly VertexBuffer _vertexBuffer;
    private SpriteFont? _font;
    private readonly SpriteBatch _spriteBatch;

    public DebuggingLayer(GraphicsDevice graphicsDevice, Camera camera)
    {
        _camera = camera;
        
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

        _vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), 6, BufferUsage.WriteOnly);
        _indexBuffer = new IndexBuffer(graphicsDevice, typeof(short), 6, BufferUsage.WriteOnly);

        _vertexBuffer.SetData(vertices);
        _indexBuffer.SetData(indices);
        
        _debugEffect = new BasicEffect(graphicsDevice);
        _spriteBatch = new SpriteBatch(graphicsDevice);
    }
    public void LoadContent(ContentManager content)
    {
        _font = content.Load<SpriteFont>("debugfont");
    }
    
    public void Update(float deltaTime)
    {

    }

    private float _time = 0;
    public void Draw(float deltaTime, GraphicsDevice graphicsDevice)
    {
        _debugEffect!.Projection = GlobalGameContext.Current.Projection;

        // axis lines in world space
        _debugEffect.View = _camera.ViewMatrix;
        _debugEffect.World = Matrix.Identity;
        _debugEffect.VertexColorEnabled = true;
        _debugEffect.CurrentTechnique.Passes[0].Apply();

        graphicsDevice.SetVertexBuffer(_vertexBuffer);
        graphicsDevice.Indices = _indexBuffer;
        graphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, 3);

        // axis lines in cam space
        _debugEffect.View = Matrix.CreateLookAt(_camera.Transform.Backward * 0.2f, Vector3.Zero, _camera.Transform.Up);
        _debugEffect.World = Matrix.CreateScale(0.01f);
        _debugEffect.CurrentTechnique.Passes[0].Apply();
        
        graphicsDevice.SetVertexBuffer(_vertexBuffer);
        graphicsDevice.Indices = _indexBuffer;
        graphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, 3);
        
        _time += (deltaTime - _time) / 5;
        
        _spriteBatch.Begin();
        _spriteBatch.DrawString(_font, $"FPS: {(1 / _time)}", new Vector2(10, 10), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
        _spriteBatch.End();
    }

}