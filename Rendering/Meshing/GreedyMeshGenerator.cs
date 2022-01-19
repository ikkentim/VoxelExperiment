using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Data;
using MyGame.Rendering.Vertices;
using MyGame.World;
using MyGame.World.Blocks;

namespace MyGame.Rendering.Meshing;

public class GreedyMeshGenerator
{
    private readonly Dictionary<Texture2D, AtlasBuffer> _bufferPerAtlas = new();
    private readonly bool _includeMeshLines;
    private readonly TextureRegistry _textureRegistry;

    public GreedyMeshGenerator(TextureRegistry textureRegistry, bool includeMeshLines)
    {
        _textureRegistry = textureRegistry;
        _includeMeshLines = includeMeshLines;

        Debug.Assert(Chunk.Size == 16); // required for using BoolArray16X16
    }
    
    public IChunkMesh Create(Chunk chunk, GraphicsDevice graphicsDevice)
    {
        foreach (var value in _bufferPerAtlas.Values)
        {
            value.Buffer.Clear();
            value.LinesBuffer?.Clear();
        }

        if (chunk.BlockCount == 0)
            return EmptyChunkMesh.Instance;

        for (var depth = 0; depth < Chunk.Size; depth++)
            foreach (var face in BlockFaces.AllFaces)
            {
                GreedyMesh(chunk, face, depth);
            }

        var parts = new List<ChunkMesh.MeshPart>(_bufferPerAtlas.Count);

        foreach (var (texture, atlasBuffer) in _bufferPerAtlas)
        {
            if (atlasBuffer.Buffer.PrimitiveCount != 0)
            {
                var (indexBuffer, vertexBuffer, primitiveCount) = atlasBuffer.Buffer.CreateBuffers(graphicsDevice);

                var part = new ChunkMesh.MeshPart
                {
                    Texture = texture,
                    IndexBuffer = indexBuffer,
                    VertexBuffer = vertexBuffer,
                    PrimitiveCount = primitiveCount,
                    TextureSize = atlasBuffer.TextureSize
                };

                if (atlasBuffer.LinesBuffer is not null)
                {
                    (indexBuffer, vertexBuffer, primitiveCount) = atlasBuffer.LinesBuffer.CreateBuffers(graphicsDevice);
                    part.LineIndexBuffer = indexBuffer;
                    part.LineVertexBuffer = vertexBuffer;
                    part.LinePrimitiveCount = primitiveCount;
                }

                parts.Add(part);
            }
        }

        return parts.Count == 0
            ? EmptyChunkMesh.Instance
            : new ChunkMesh(parts);
    }

    private static bool IsFaceVisible(Chunk chunk, IntVector3 localPos, IntVector3 normal) => !chunk.GetRelativeBlock(localPos + normal).BlockType!.IsOpaque;

    private void GreedyMesh(Chunk chunk, BlockFace blockFace, int depth)
    {
        var visited = new BoolArray16X16();
        var normal = BlockFaces.GetNormal(blockFace);

        for (var j = 0; j < Chunk.Size; j++)
        for (var i = 0; i < Chunk.Size; i++)
        {
            if (visited[i, j])
                continue;

            GreedyMeshFromPoint(chunk, blockFace, normal, depth, i, j, ref visited);
        }
    }
    
    private void GreedyMeshFromPoint(Chunk chunk, BlockFace blockFace, IntVector3 normal, int depth, int i, int j, ref BoolArray16X16 visited)
    {
        var localBlockPosition = GetPosition(blockFace, depth, i, j);
        var source = chunk.GetBlock(localBlockPosition);
        
        if (source.BlockType is AirBlock || !IsFaceVisible(chunk, localBlockPosition, normal))
        {
            // Face not visible - skip
            visited[i, j] = true;
            return;
        }

        // The texture we'll be merging in the current routine
        var sourceTexture = source.BlockType!.GetTexture(blockFace).Name;

        // Expand covered area towards i+
        var maxI = i;
        for (var i2 = i + 1; i2 < Chunk.Size; i2++)
        {
            if (visited[i2, j])
            {
                // face was already visited somehow (shouldn't happen?)
                break;
            }

            var blockPos = GetPosition(blockFace, depth, i2, j);
            var block = chunk.GetBlock(blockPos);

            
            if (!IsFaceVisible(chunk, blockPos, normal))
            {
                // face is not visible
                visited[i2, j] = true;
                break;
            }

            if (block.BlockType!.GetTexture(blockFace).Name != sourceTexture)
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

                var blockPos = GetPosition(blockFace, depth, i2, j2);
                var block = chunk.GetBlock(blockPos);

                
                if (!IsFaceVisible(chunk, blockPos, normal))
                {
                    // face is not visible
                    visited[i2, j2] = true;
                    accept = false;
                    break;
                }

                if (block.BlockType!.GetTexture(blockFace).Name != sourceTexture)
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
                buffer = new AtlasBuffer(new BufferGeneratorV2<VertexPositionBlockFace>(),
                    _includeMeshLines ? new BufferGeneratorV2<VertexPosition>() : null,
                    textureReference.UvSize);
        }

        AddFaces(blockFace, normal, i, j, maxI, maxJ, localBlockPosition, buffer, textureReference);
    }

    private static void AddFaces(BlockFace blockFace, IntVector3 normal, int i, int j, int maxI, int maxJ, IntVector3 localPos, AtlasBuffer buffer,
        TextureAtlasReference textureReference)
    {
        var lenI = maxI - i + 1;
        var lenJ = maxJ - j + 1;
        var size = new Vector2(lenI, lenJ);

        var faceSize = GetPosition(blockFace, 0, lenI, lenJ);
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

            return new VertexPositionBlockFace(position, normal, uv, textureReference.Uv);
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

    private record AtlasBuffer(BufferGeneratorV2<VertexPositionBlockFace> Buffer, BufferGeneratorV2<VertexPosition>? LinesBuffer,
        Vector2 TextureSize);
}