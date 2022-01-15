﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Data;
using MyGame.Rendering.Vertices;
using MyGame.World;

namespace MyGame.Rendering.Meshing;

public class GreedyMeshGenerator : IDisposable
{
    private readonly Dictionary<Texture2D, AtlasBuffer> _bufferPerAtlas = new();
    private readonly Chunk _chunk;
    private readonly bool _includeMeshLines;
    private readonly TextureRegistry _textureRegistry;

    public GreedyMeshGenerator(Chunk chunk, TextureRegistry textureRegistry, bool includeMeshLines)
    {
        _chunk = chunk;
        _textureRegistry = textureRegistry;
        _includeMeshLines = includeMeshLines;

        Debug.Assert(Chunk.Size == 16); // required for using BoolArray16X16
    }

    public void Dispose()
    {
        foreach (var value in _bufferPerAtlas.Values)
        {
            value.Buffer.Dispose();
            value.LinesBuffer?.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    public ChunkMesh Create(GraphicsDevice graphicsDevice)
    {
        foreach (var kv in _bufferPerAtlas)
        {
            kv.Value.Buffer.Clear();
            kv.Value.LinesBuffer?.Clear();
        }

        for (var depth = 0; depth < Chunk.Size; depth++)
            foreach (var face in BlockFaces.AllFaces)
            {
                GreedyMesh(face, depth);
            }

        var parts = new List<ChunkMesh.MeshPart>(_bufferPerAtlas.Count);

        foreach (var (texture, atlasBuffer) in _bufferPerAtlas)
        {
            if (!atlasBuffer.Buffer.IsEmpty)
            {
                var (indexBuffer, vertexBuffer) = atlasBuffer.Buffer.GetBuffers(graphicsDevice);

                var part = new ChunkMesh.MeshPart
                {
                    Texture = texture,
                    IndexBuffer = indexBuffer,
                    VertexBuffer = vertexBuffer,
                    PrimitiveCount = atlasBuffer.Buffer.PrimitiveCount,
                    TextureSize = atlasBuffer.TextureSize
                };

                if (atlasBuffer.LinesBuffer is { IsEmpty: false })
                {
                    (indexBuffer, vertexBuffer) = atlasBuffer.LinesBuffer.GetBuffers(graphicsDevice);
                    part.LineIndexBuffer = indexBuffer;
                    part.LineVertexBuffer = vertexBuffer;
                    part.LinePrimitiveCount = atlasBuffer.LinesBuffer.PrimitiveCount;
                }

                parts.Add(part);
            }
        }

        return new ChunkMesh(parts);
    }

    private void GreedyMesh(BlockFace blockFace, int depth)
    {
        var visited = new BoolArray16X16();

        for (var j = 0; j < Chunk.Size; j++)
        for (var i = 0; i < Chunk.Size; i++)
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
        var sourceTexture = source.Kind!.GetTexture(blockFace).Name;

        // Expand covered area towards i+
        var maxI = i;
        for (var i2 = i + 1; i2 < Chunk.Size; i2++)
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

            if (block.Kind!.GetTexture(blockFace).Name != sourceTexture)
            {
                // face has a different texture
                break;
            }

            visited[i2, j] = true;
            maxI++;
        }

        // Expand covered area towards j+
        var maxJ = j;
        for (var j2 = j + 1; j2 < Chunk.Size; j2++)
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

                if (block.Kind!.GetTexture(blockFace).Name != sourceTexture)
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

        var textureReference = _textureRegistry.GetTexture(sourceTexture);

        if (!_bufferPerAtlas.TryGetValue(textureReference.Texture, out var buffer))
        {
            _bufferPerAtlas[textureReference.Texture] =
                buffer = new AtlasBuffer(new BufferGenerator<VertexPositionBlockFace>(),
                    _includeMeshLines ? new BufferGenerator<VertexPosition>() : null,
                    textureReference.UvSize);
        }

        AddFaces(blockFace, i, j, maxI, maxJ, localBlockPosition, buffer, textureReference);
    }

    private void AddFaces(BlockFace blockFace, int i, int j, int maxI, int maxJ, IntVector3 localPos, AtlasBuffer buffer,
        TextureAtlasReference textureReference)
    {
        var lenI = maxI - i + 1;
        var lenJ = maxJ - j + 1;
        var size = new Vector2(lenI, lenJ);

        var faceSize = GetPosition(blockFace, 0, lenI, lenJ);
        var normal = BlockFaces.GetNormal(blockFace);
        var up = BlockFaces.GetUp(blockFace);
        var cross = Vector3.Cross(normal, up);
        var absCross = VectorHelper.Abs(cross);

        var faceTopRight = localPos
                           // Get center
                           + (absCross + up) * (faceSize / 2)

                           // Get top-right
                           + (cross + up) * (faceSize / 2);

        if (BlockFaces.IsPositive(blockFace))
        {
            faceTopRight += normal;
        }

        VertexPositionBlockFace ToVertex(Vector2 uv)
        {
            var position = faceTopRight + (cross * -(1 - uv.X) + up * -uv.Y) * faceSize;

            uv.X = 1 - uv.X; // invert x for now...
            uv *= size;

            return new VertexPositionBlockFace(position, uv, textureReference.Uv);
        }

        if (buffer.LinesBuffer != null)
        {
            VertexPosition ToLinesVertex(Vector2 uv)
            {
                var position = faceTopRight + (cross * -(1 - uv.X) + up * -uv.Y) * faceSize;
                return new VertexPosition(position);
            }

            buffer.LinesBuffer.AddFaceLines(
                ToLinesVertex(new Vector2(0, 0)),
                ToLinesVertex(new Vector2(1, 0)),
                ToLinesVertex(new Vector2(0, 1)),
                ToLinesVertex(new Vector2(1, 1))
            );
        }

        buffer.Buffer.AddFace(
            ToVertex(new Vector2(0, 0)),
            ToVertex(new Vector2(1, 0)),
            ToVertex(new Vector2(0, 1)),
            ToVertex(new Vector2(1, 1))
        );
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

    private record AtlasBuffer(BufferGenerator<VertexPositionBlockFace> Buffer, BufferGenerator<VertexPosition>? LinesBuffer,
        Vector2 TextureSize);
}