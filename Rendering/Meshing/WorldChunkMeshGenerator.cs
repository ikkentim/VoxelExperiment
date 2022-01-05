using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Data;
using MyGame.Extensions;
using MyGame.World;

namespace MyGame.Rendering.Meshing;

public class WorldChunkMeshGenerator
{
    private readonly Chunk _chunk;
    private readonly TextureProvider _textureProvider;
    private readonly bool _isLines;
    private readonly Dictionary<Texture2D, BufferGenerator<VertexPositionTexture>> _bufferPerAtlas = new();

    public WorldChunkMeshGenerator(Chunk chunk, TextureProvider textureProvider, bool isLines)
    {
        _chunk = chunk;
        _textureProvider = textureProvider;
        _isLines = isLines;

        Debug.Assert(Chunk.ChunkSize == 16); // required for using BoolArray16X16
    }

    public ChunkMesh Create(GraphicsDevice graphicsDevice)
    {
        foreach (var kv in _bufferPerAtlas)
        {
            kv.Value.Clear();
        }
            
        for (var depth = 0; depth < Chunk.ChunkSize; depth++)
            foreach (var face in BlockFaces.AllFaces)
            {
                GreedyMesh(face, depth);
            }

        var parts = new List<ChunkMesh.MeshPart>(_bufferPerAtlas.Count);

        foreach (var (texture, buffers) in _bufferPerAtlas)
        {
            if (!buffers.IsEmpty)
            {
                var (indexBuffer, vertexBuffer) = buffers.GetBuffers(graphicsDevice);

                parts.Add(new ChunkMesh.MeshPart
                {
                    Texture = texture,
                    IndexBuffer = indexBuffer,
                    VertexBuffer = vertexBuffer,
                    PrimitiveCount = buffers.PrimitiveCount
                });
            }
        }

        return new ChunkMesh(parts, _isLines);
    }

    private void GreedyMesh(BlockFace blockFace, int depth)
    {
        var visited = new BoolArray16X16();
            
        for (var j = 0; j < Chunk.ChunkSize; j++)
        for (var i = 0; i < Chunk.ChunkSize; i++)
        {
            if (visited[i, j])
                continue;

            GreedyMeshFromPoint(blockFace, depth, i, j, ref visited);
        }
    }

    private void GreedyMeshFromPoint(BlockFace blockFace, int depth, int i, int j, ref BoolArray16X16 visited)
    {
        var localBlockPosition = GetPosition(blockFace, depth, i, j);
        var source = _chunk.GetBlock(localBlockPosition);

        if ((source.VisibleBlockFaces & blockFace) == BlockFace.None)
        {
            // Face not visible - skip
            visited[i, j] = true;
            return;
        }
            
        // The texture we'll be merging in the current routine
        var sourceTexture = source.Kind!.GetTexture().Name;

        // Expand covered area towards i+
        var maxI = i;
        for (var i2 = i + 1; i2 < Chunk.ChunkSize; i2++)
        {
            if (visited[i2, j])
            { 
                // face was already visited somehow (shouldn't happen?)
                break;
            }

            var block = _chunk.GetBlock(GetPosition(blockFace, depth, i2, j));

            if ((block.VisibleBlockFaces & blockFace) == BlockFace.None)
            {
                // face is not visible
                visited[i2, j] = true;
                break;
            }

            if (block.Kind!.GetTexture().Name != sourceTexture)
            {
                // face has a different texture
                break;
            }
                
            visited[i2, j] = true;
            maxI++;
        }

        // Expand covered area towards j+
        var maxJ = j;
        for (var j2 = j + 1; j2 < Chunk.ChunkSize; j2++)
        {
            var accept = true;

            // check every face between [i - maxI] in column j2
            for (var i2 = i; i2 <= maxI; i2++)
            {
                if (visited[i2, j2])
                {
                    // face already visited
                    accept = false;
                    break;
                }
                    
                var block = _chunk.GetBlock(GetPosition(blockFace, depth, i2, j2));
                    
                if ((block.VisibleBlockFaces & blockFace) == BlockFace.None)
                {
                    // face is not visible
                    visited[i2, j2] = true;
                    accept = false;
                    break;
                }

                if (block.Kind!.GetTexture().Name != sourceTexture)
                {
                    // face has a different texture
                    accept = false;
                    break;
                }
            }

            if (!accept)
            {
                break;
            }

            // mark every face in the column as visited
            for (var i2 = i; i2 <= maxI; i2++)
            {
                visited[i2, j2] = true;
            }

            maxJ++;
        }
            
        var buffer = GetBuffer(sourceTexture);
        AddFaces(blockFace, i, j, maxI, maxJ, localBlockPosition, buffer);
    }

    private void AddFaces(BlockFace blockFace, int i, int j, int maxI, int maxJ, IntVector3 localPos, BufferGenerator<VertexPositionTexture> buffer)
    {
        var lenI = maxI - i + 1;
        var lenJ = maxJ - j + 1;
        var size = new Vector2(lenI, lenJ);
            
        var faceSize = GetPosition(blockFace, 0, lenI, lenJ);
        var normal = BlockFaces.GetNormal(blockFace);
        var up = BlockFaces.GetUp(blockFace);
            
        var cross = Vector3.Cross(normal, up);
        var absCross = new Vector3(
            MathF.Abs(cross.X),
            MathF.Abs(cross.Y),
            MathF.Abs(cross.Z)
        );

        var faceTopRight = localPos
                           // Get center
                           + (absCross + up) * (faceSize / 2)

                           // Get top-right
                           + (cross + up) * (faceSize / 2);
            
        if (BlockFaces.IsPositive(blockFace))
        {
            faceTopRight += normal;
        }
            
        VertexPositionTexture ToVertex(Vector2 uv)
        {
            var position = faceTopRight + (cross * -(1 - uv.X) + up * -uv.Y) * faceSize;

            uv.X = 1 - uv.X; // invert x for now...
            uv *= size;

            return new VertexPositionTexture(position, uv);
        }
            
        if (_isLines)
        {
            buffer.AddFaceLines(
                ToVertex(new Vector2(0, 0)),
                ToVertex(new Vector2(1, 0)),
                ToVertex(new Vector2(0, 1)),
                ToVertex(new Vector2(1, 1))
            );
        }
        else
        {
            buffer.AddFace(
                ToVertex(new Vector2(0, 0)),
                ToVertex(new Vector2(1, 0)),
                ToVertex(new Vector2(0, 1)),
                ToVertex(new Vector2(1, 1))
            );
        }
    }

    private BufferGenerator<VertexPositionTexture> GetBuffer(string textureName)
    {
        var atlas = _textureProvider.GetTexture(textureName);
        return _bufferPerAtlas.GetOrAdd(atlas);
    }
        
    private static IntVector3 GetPosition(BlockFace blockFace, int depth, int i, int j)
    {
        var normal = BlockFaces.GetNormal(blockFace);
        var pos = normal * depth;
            
        if (BlockFaces.IsNegative(blockFace))
            pos = -pos;
            
        if (normal.X != 0)
        {
            pos.Y = j;
            pos.Z = i;
        }
        else if (normal.Y != 0)
        {
            pos.X = i;
            pos.Z = j;
        }
        else
        {
            pos.X = i;
            pos.Y = j;
        }

        return pos;
    }
}