using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Accessibility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MyGame.World.Rendering;

public class NewWorldChunkRenderer : IWorldChunkRenderer
{
    private readonly WorldChunk _chunk;
    private readonly WorldChunkRendererResources _rendererResources;

    public NewWorldChunkRenderer(WorldChunk chunk, WorldChunkRendererResources rendererResources)
    {
        _chunk = chunk;
        _rendererResources = rendererResources;
    }

    private List<GreedyMesh> _layers = new();

    public void Initialize()
    {
        _layers.Add(new GreedyMesh(_chunk, _rendererResources, Face.West, 0));
    }

    public void BlockUpdated(IntVector3 localPosition)
    {
        throw new NotImplementedException();
    }

    public void Draw(GraphicsDevice graphicsDevice)
    {
        foreach (var mesh in _layers)
        {
            mesh.Draw(graphicsDevice);
        }
    }
}

public class GreedyMesh
{
    private readonly WorldChunk _chunk;
    private readonly WorldChunkRendererResources _resources;
    private readonly Face _face;
    private readonly IntVector3 _normal;
    private readonly int _index;
    
    public GreedyMesh(WorldChunk chunk, WorldChunkRendererResources resources, Face face, int index)
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
                if (visited[i + j * WorldChunk.ChunkSize])
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

    private void GreedyFrom(ref int i, ref int j, ref BoolArray16X16 visited)
    {
        if (GetBlock(i, j).Kind == null)
        {
            // nothing to see here
            visited[i + j * WorldChunk.ChunkSize] = true;
            return;
        }

        // Expand towards i+
        int maxI = i;
        for (var i2 = i; i2 < WorldChunk.ChunkSize; i2++)
        {
            if (GetBlock(i2, j).Kind == null)
            {
                // it's air, no further
                visited[i2 + j * WorldChunk.ChunkSize] = true;
                break;
            }
            else
            {
                maxI++;
            }
        }

        // Expand towards j+
        var maxJ = j;
        for (var j2 = j; j2 < WorldChunk.ChunkSize; j2++)
        {
            var bad = false;
            // got to check every block between (i - maxI) on column j2
            for (var i2 = i; i2 <= maxI; i2++)
            {
                if (GetBlock(i2, j2).Kind == null)
                {
                    // it's air, no further
                    visited[i2 + j2 * WorldChunk.ChunkSize] = true;
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
            visited[i2 + j2 * WorldChunk.ChunkSize] = true;

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

        // optimize outer loop
        if (maxJ == WorldChunk.ChunkSize - 1 && j == 0)
        {
            i = maxI;
        }

        j = maxJ;
    }

    private static Vector3 Absolute(Vector3 normal)
    {
        return new Vector3(
            MathF.Abs(normal.X),
            MathF.Abs(normal.Y),
            MathF.Abs(normal.Z)
        );
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

    private static readonly short[] FaceIndicesLine = {
        2, 0, 1, 3, 2, 1
    };

    public void Draw(GraphicsDevice graphicsDevice)
    {
        var basicEffect = _resources.BasicEffect;
        basicEffect.TextureEnabled = false;

        foreach (var mesh in _meshes)
        {
            var matrix = Matrix.Identity;
            
            basicEffect.World = matrix * Matrix.CreateTranslation(mesh.Position);

            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineStrip, FaceVerticesLine, 0, FaceVerticesLine.Length, FaceIndicesLine, 0, 5);
            }
        }
    }
}

public struct BoolArray16X16
{
    private ulong _a;
    private ulong _b;
    private ulong _c;
    private ulong _d;

    public bool this[int index]
    {
        get => Get(index);
        set => Set(index, value);
    }

    public bool Get(int index)
    {
        if (index < 0 || index >= 256)
            throw new ArgumentOutOfRangeException(nameof(index));

        var rem = index % 4;
        switch (index / 4)
        {
            case 0: return ((_a >> rem) & 1) == 1;
            case 1: return ((_b >> rem) & 1) == 1;
            case 2: return ((_c >> rem) & 1) == 1;
            default: return ((_d >> rem) & 1) == 1;
        }
    }

    public void Set(int index, bool value)
    {
        if (index < 0 || index >= 256)
            throw new ArgumentOutOfRangeException(nameof(index));

        var rem = index % 4;
        switch (index / 4)
        {
            case 0: 
                _a &= (ulong)(value ? 1 << rem : 0);
                break;
            case 1:
                _b &= (ulong)(value ? 1 << rem : 0);
                break;
            case 2: 
                _c &= (ulong)(value ? 1 << rem : 0);
                break;
            default: 
                _d &= (ulong)(value ? 1 << rem : 0);
                break;
        }
    }
    
}