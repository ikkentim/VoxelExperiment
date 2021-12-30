using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MyGame.World;

public class WorldChunkRenderer
{
    private readonly WorldChunk _chunk;
    private readonly WorldChunkRendererResources _resources;

    private VertexPositionTexture[] _vertices;
    private short[] _indices;

    public WorldChunkRenderer(WorldChunk chunk, WorldChunkRendererResources resources)
    {
        _chunk = chunk;
        _resources = resources;

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

        _vertices = new[]
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

        _indices = new short[]
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
    }

    public void BlockUpdated(IntVector3 localPosition)
    {
        // rebuild meshes
    }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        for (var x = 0; x < WorldChunk.ChunkSize; x++)
        {
            for (var y = 0; y < WorldChunk.ChunkSize; y++)
            {
                for (var z = 0; z < WorldChunk.ChunkSize; z++)
                {
                    var block = _chunk.GetBlock(new IntVector3(x, y, z));
                    var pos = _chunk.GetBlockPosition(new IntVector3(x, y, z));

                    if (block.Kind != null)
                    {
                        _resources.BasicEffect.World = Matrix.CreateTranslation(pos);

                        foreach (var pass in _resources.BasicEffect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            //graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 12);

                            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _vertices, 0, _vertices.Length, _indices, 0, 12);
                        }
                    }
                }
            }
        }
    }
    
    private void DrawFace(GraphicsDevice graphicsDevice)
    {

    }
}

public class WorldChunkRendererResources
{
    private BasicEffect? _basicEffect;

    // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
    public BasicEffect BasicEffect => _basicEffect!;

    private Texture2D? _testTexture;

    public void Initialize(GraphicsDevice graphicsDevice)
    {
        _basicEffect = new BasicEffect(graphicsDevice);
        _basicEffect.TextureEnabled = true;
    }

    public void LoadContent(ContentManager content)
    {
        _testTexture = content.Load<Texture2D>("checkered");
        BasicEffect.Texture = _testTexture;
    }
}