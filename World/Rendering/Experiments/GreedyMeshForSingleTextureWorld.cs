using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyGame.World.Blocks;

namespace MyGame.World.Rendering.Experiments;

public class GreedyMeshForSingleTextureWorld
{
    private readonly WorldChunk _chunk;
    private readonly WorldChunkRendererResources _resources;
    private readonly Face _face;
    private readonly IntVector3 _normal;
    private readonly int _index;
    
    public GreedyMeshForSingleTextureWorld(WorldChunk chunk, WorldChunkRendererResources resources, Face face, int index)
    {
        if (!Enum.IsDefined(typeof(Face), face) || face == Face.None) throw new InvalidEnumArgumentException(nameof(face), (int)face, typeof(Face));

        _chunk = chunk;
        _resources = resources;
        _face = face;
        _normal = Faces.GetNormal(_face);
        _index = index;
    }

    public void Build()
    {
        Debug.Assert(WorldChunk.ChunkSize == 16); // required for using BoolArray16X16

        var visited = new BoolArray16X16();
        for (var i = 0; i < WorldChunk.ChunkSize; i++)
        {
            for (var j = 0; j < WorldChunk.ChunkSize; j++)
            {
                if (visited[i, j])
                    continue;

                GreedyFrom(ref i, ref j, ref visited);
            }
        }
    }

    private BlockData GetBlock(int i, int j)
    {
        var localBlockPos = GetLocalBlockPosition(i, j);
        return _chunk.GetBlock(localBlockPos);
    }

    private readonly List<MeshData> _meshes = new();

    public int MeshCount => _meshes.Count;

    private bool BlockHasVisibleFace(int i, int j)
    {
        var block = GetBlock(i, j);

        return block.Kind != null && block.VisibleFaces.HasFlag(_face);
    }
    private void GreedyFrom(ref int i, ref int j, ref BoolArray16X16 visited)
    {
        if (!BlockHasVisibleFace(i, j))
        {
            // nothing to see here
            visited[i, j] = true;
            return;
        }

        // Expand towards i+
        int maxI = i;
        for (var i2 = i + 1; i2 < WorldChunk.ChunkSize; i2++)
        {
            if (visited[i2, j] || !BlockHasVisibleFace(i2, j))
            {
                // it's air, no further
                visited[i2, j] = true;
                break;
            }
            else
            {
                maxI++;
            }
        }

        // Expand towards j+
        var maxJ = j;
        for (var j2 = j + 1; j2 < WorldChunk.ChunkSize; j2++)
        {
            var bad = false;
            // got to check every block between (i - maxI) on column j2
            for (var i2 = i; i2 <= maxI; i2++)
            {
                if (visited[i2, j2] || !BlockHasVisibleFace(i2, j2))
                {
                    // it's air, no further
                    visited[i2, j2] = true;
                    bad = true;
                    break;
                }
            }

            if (bad)
            {
                break;
            }
            else
            {
                maxJ++;
            }
        }
        
        // Visit everything
        for(var i2 = i; i2 <= maxI; i2++)
        for (var j2 = j; j2 <= maxJ; j2++)
            visited[i2, j2] = true;

        // add mesh
        var meshPos = GetLocalBlockPosition(i, j);
        
        if ((_face & Faces.PositiveFaces) != Face.None)
        {
            meshPos += _normal;
        }


        var size = Vector2.One + new Vector2(maxI - i, maxJ - j);
        
        _meshes.Add(new MeshData
        {
            Position = _chunk.WorldPosition + meshPos,
            Size = size
        });
    }

    private IntVector3 GetLocalBlockPosition(int i, int j)
    {
        switch (_face)
        {
            case Face.Top:
            case Face.Bottom:
                return new IntVector3(i, _index, j);
            case Face.South:
            case Face.North:
                return new IntVector3(i, j, _index);
            case Face.West:
            case Face.East:
                return new IntVector3(_index, i, j);
            default:
                throw new InvalidOperationException();
        }
    }
    private IntVector3 GetLocalScale(int i, int j)
    {
        switch (_face)
        {
            case Face.Top:
            case Face.Bottom:
                return new IntVector3(i, 0, j);
            case Face.South:
            case Face.North:
                return new IntVector3(i, j, 0);
            case Face.West:
            case Face.East:
                return new IntVector3(0, i, j);
            default:
                throw new InvalidOperationException();
        }
    }

    public void BlockUpdated(IntVector3 localPosition)
    {
        throw new NotImplementedException();
    }

    private struct MeshData
    {
        public Vector3 Position;
        public Vector2 Size;
    }
    
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
    private readonly VertexPositionTexture[] _faceVertices = {
        new(new Vector3(0, 0, 0), new Vector2(0, 0)),
        new(new Vector3(1, 0, 0), new Vector2(1, 0)),
        new(new Vector3(0, 1, 0), new Vector2(0, 1)),
        new(new Vector3(1, 1, 0), new Vector2(1, 1)),
    };

    private static readonly short[] FaceIndicesLine = {
        2, 0, 1, 3, 2, 1
    };

    public void Draw(GraphicsDevice graphicsDevice)
    {
        var basicEffect = _resources.BasicEffect;
        basicEffect.TextureEnabled = false;

        foreach (var mesh in _meshes)
        {
            graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            
            basicEffect.World = GetMatrixForFaceMesh(mesh.Position, mesh.Size);

            _faceVertices[1].TextureCoordinate.X = mesh.Size.X;
            _faceVertices[2].TextureCoordinate.Y = mesh.Size.Y;
            _faceVertices[3].TextureCoordinate.X = mesh.Size.X;
            _faceVertices[3].TextureCoordinate.Y = mesh.Size.Y;

            basicEffect.TextureEnabled = true;
            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _faceVertices, 0, _faceVertices.Length, FaceIndices, 0, 2);
            }

            basicEffect.TextureEnabled = false;
            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineStrip, FaceVerticesLine, 0, FaceVerticesLine.Length, FaceIndicesLine, 0, 5);
            }
        }
    }

    private Matrix GetMatrixForFaceMesh(Vector3 meshPosition, Vector2 scale)
    {
        var blockFaceNormal = _normal;

        // around origin of mesh
        var world = Matrix.CreateTranslation(-.5f, -.5f, 0);
        
        // rotate to normal of block face
        if (blockFaceNormal.Z == 0)
        {
            var axis = new Vector3(blockFaceNormal.Y, -blockFaceNormal.X, 0);

            world *= Matrix.CreateFromAxisAngle(axis, MathHelper.PiOver2);
        }
        else if (blockFaceNormal.Z == 1)
        {
            world *= Matrix.CreateRotationY(MathHelper.Pi);
        }

        // scale and translate to position
        var scaleOnPlane = GetLocalScale((int)scale.X, (int)scale.Y); // shouldn't have to cast...
        var tr = scaleOnPlane * 0.5f;
        world *= Matrix.CreateScale(scaleOnPlane);
        world *= Matrix.CreateTranslation(tr);
        world *= Matrix.CreateTranslation(meshPosition);
        
        return world;
    }
}