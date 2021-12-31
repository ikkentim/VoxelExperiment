using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyGame.World.Rendering;

public class SimplePerSquareMesh
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

    public SimplePerSquareMesh(IntVector3 chunkPosition, IntVector3 normal, int index)
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
                    graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineStrip, FaceVerticesLine, 0, FaceVerticesLine.Length, FaceIndicesLine, 0, 5);
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