using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace MyGame.World;

public class WorldChunkRenderer
{
    private readonly WorldChunk _chunk;
    private readonly WorldChunkRendererResources _resources;

    private VertexPositionTexture[] _vertices;
    private short[] _indices;


    private VertexPositionTexture[] _faceVertices;
    private short[] _faceIndices;
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

        _faceVertices = new[]
        {
            new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3(1, 0, 0), new Vector2(1, 0)),
            new VertexPositionTexture(new Vector3(0, 1, 0), new Vector2(0, 1)),
            new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 1)),
        };

        _faceIndices = new short[]
        {
            0, 1, 2,
            1, 3, 2,
        };
    }

    private readonly List<MeshLayer> _layers = new();
    public void Initialize()
    {
        // initial mesh build
        InitializeLayers(IntVector3.UnitX);
        InitializeLayers(IntVector3.UnitY);
        InitializeLayers(IntVector3.UnitZ);
    }

    private void InitializeLayers(IntVector3 normal)
    {
        for (var i = 0; i <= WorldChunk.ChunkSize; i++)
        {
            var layer = new MeshLayer(_chunk.Position, normal, i);

            layer.Build(_chunk);

            _layers.Add(layer);
        }
    }
    
    public void BlockUpdated(IntVector3 localPosition)
    {
        // rebuild meshes
        // todo
    }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        DrawAllMeshLayers(graphicsDevice);
    }
    
    private void DrawAllMeshLayers(GraphicsDevice graphicsDevice)
    {
        foreach (var m in _layers)
        {
            m.Draw(graphicsDevice, _resources.BasicEffect);
        }
    }

    private void DrawAllCubes(GraphicsDevice graphicsDevice)
    {
        for (var xLayer = 0; xLayer < WorldChunk.ChunkSize; xLayer++)
        {
            for (var y = 0; y < WorldChunk.ChunkSize; y++)
            {
                for (var z = 0; z < WorldChunk.ChunkSize; z++)
                {
                    var block = _chunk.GetBlock(new IntVector3(xLayer, y, z));
                    var pos = _chunk.GetBlockPosition(new IntVector3(xLayer, y, z));

                    if (block.Kind != null)
                    {
                        _resources.BasicEffect.World = Matrix.CreateTranslation(pos);

                        foreach (var pass in _resources.BasicEffect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _vertices, 0, _vertices.Length, _indices, 0, 12);
                        }
                    }
                }
            }
        }
    }

}

public class MeshLayer
{
    private readonly IntVector3 _chunkPosition;
    private readonly IntVector3 _normal;
    private readonly int _index;

    private readonly MeshFace[,] _faces;
    
    private static readonly VertexPositionTexture[] FaceVertices = {
        new(new Vector3(0, 0, 0), new Vector2(0, 0)),
        new(new Vector3(1, 0, 0), new Vector2(1, 0)),
        new(new Vector3(0, 1, 0), new Vector2(0, 1)),
        new(new Vector3(1, 1, 0), new Vector2(1, 1)),
    };
    private static readonly VertexPosition[] FaceVerticesLine = {
        new(new Vector3(0, 0, 0)),
        new(new Vector3(1, 0, 0)),
        new(new Vector3(0, 1, 0)),
        new(new Vector3(1, 1, 0))
    };

    private static readonly short[] FaceIndices = {
        0, 1, 2,
        1, 3, 2,
    };

    private static readonly short[] FaceIndicesLine = {
        2, 0, 1, 3, 2, 1
    };

    public MeshLayer(IntVector3 chunkPosition, IntVector3 normal, int index)
    {
        _chunkPosition = chunkPosition;
        _index = index;
        _normal = normal;
        _faces = new MeshFace[WorldChunk.ChunkSize, WorldChunk.ChunkSize];
    }

    public void Build(WorldChunk chunk)
    {
        var count = 0;
        var inv = 0;
        for (var i = 0; i < WorldChunk.ChunkSize; i++)
        {
            for (var j = 0; j < WorldChunk.ChunkSize; j++)
            {
                _faces[i, j] = GetFace(chunk, i, j);

                if (_faces[i, j].Block != null)
                {
                    count++;

                    if (_faces[i, j].Inverted) inv++;
                }
            }
        }
        
        Debug.WriteLine($"layer {_index} @ {_chunkPosition} has {count} faces of which {inv} are inverted");
    }

    public void Draw(GraphicsDevice graphicsDevice, BasicEffect basicEffect)
    {
        for (var i = 0; i < WorldChunk.ChunkSize; i++)
        {
            for (var j = 0; j < WorldChunk.ChunkSize; j++)
            {
                var face = _faces[i, j];

                if (face.Block == null)
                {
                    // nothing to see here
                    continue;
                }

                var pos = (Vector3)_chunkPosition + GetPositionLocalToChunk(i, j);
                
                // default face normal is (0,0,-1)

                var rotMat = Matrix.Identity;
                
                //rotMat *= Matrix.CreateTranslation(new Vector3(-0.5f, -0.5f, 0));

                if (_normal == IntVector3.UnitX)
                {
                    rotMat *= Matrix.CreateFromAxisAngle(Vector3.UnitY, face.Inverted ? -MathHelper.PiOver2 : MathHelper.PiOver2);

                    if (!face.Inverted)
                        rotMat *= Matrix.CreateTranslation(0, 0, 1);
                }
                if (_normal == IntVector3.UnitY)
                {
                    rotMat *= Matrix.CreateFromAxisAngle(Vector3.UnitX, face.Inverted ? MathHelper.PiOver2 : -MathHelper.PiOver2);

                    if (!face.Inverted)
                        rotMat *= Matrix.CreateTranslation(0, 0, 1);
                }
                if (_normal == IntVector3.UnitZ && face.Inverted)
                {
                    rotMat *= Matrix.CreateFromAxisAngle(Vector3.UnitX, MathHelper.Pi);
                    rotMat *= Matrix.CreateTranslation(0, 1, 0);
                }

                //rotMat *= Matrix.CreateTranslation(new Vector3(0.5f, 0.5f, 0));

                basicEffect.World = rotMat * Matrix.CreateTranslation(pos);

                basicEffect.TextureEnabled = true;
                foreach (var pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, FaceVertices, 0, FaceVertices.Length, FaceIndices, 0, 2);
                }
                
                basicEffect.TextureEnabled = false;
                foreach (var pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineStrip, FaceVerticesLine, 0, FaceVertices.Length, FaceIndicesLine, 0, 5);
                }
            }
        }
    }

    private IntVector3 GetPositionLocalToChunk(int i, int j)
    {
        var localPosition = _normal * _index;

        // only one of these is true...
        if (_normal.X == 1)
        {
            localPosition.Y = i;
            localPosition.Z = j;
        }
        
        if (_normal.Y == 1)
        {
            localPosition.X = i;
            localPosition.Z = j;
        }
        
        if (_normal.Z == 1)
        {
            localPosition.X = i;
            localPosition.Y = j;
        }

        return localPosition;
    }

    private MeshFace GetFace(WorldChunk chunk, int i, int j)
    {
        var localPosition = GetPositionLocalToChunk(i, j);
        
        switch (_index)
        {
            case 0:
                return new MeshFace
                {
                    Block = chunk.GetBlock(localPosition).Kind,
                };
            case WorldChunk.ChunkSize:
                return new MeshFace
                {
                    Block = chunk.GetBlock(localPosition - _normal).Kind,
                    Inverted = true
                };
            default:
                var face1 = chunk.GetBlock(localPosition);
                var face2 = chunk.GetBlock(localPosition - _normal);

                if (face1.Kind != null && face2.Kind != null)
                {
                    return default;
                }

                if (face1.Kind != null)
                {
                    return new MeshFace
                    {
                        Block = face1.Kind,
                    };
                }

                if (face2.Kind != null)
                {
                    return new MeshFace
                    {
                        Block = face2.Kind,
                        Inverted = true
                    };
                }

                return default;
        }
    }

    

    private struct MeshFace
    {
        public BlockBase? Block;
        public bool Inverted;
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