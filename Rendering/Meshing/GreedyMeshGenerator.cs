using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Data;
using MyGame.Extensions;
using MyGame.Rendering.Vertices;
using MyGame.World;

namespace MyGame.Rendering.Meshing;

public class GreedyMeshGenerator
{
    private readonly Chunk _chunk;
    private readonly TextureRegistry _textureRegistry;
    private readonly bool _isLines;
    private readonly Dictionary<Texture2D, AtlasBuffer> _bufferPerAtlas = new();

    private record AtlasBuffer(BufferGenerator<VertexPositionBlockFace> Buffer, Vector2 TextureSize);

    public GreedyMeshGenerator(Chunk chunk, TextureRegistry textureRegistry, bool isLines)
    {
        _chunk = chunk;
        _textureRegistry = textureRegistry;
        _isLines = isLines;

        Debug.Assert(Chunk.Size == 16); // required for using BoolArray16X16
    }

    public ChunkMesh Create(GraphicsDevice graphicsDevice)
    {
        foreach (var kv in _bufferPerAtlas)
        {
            kv.Value.Buffer.Clear();
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

                parts.Add(new ChunkMesh.MeshPart
                {
                    Texture = texture,
                    IndexBuffer = indexBuffer,
                    VertexBuffer = vertexBuffer,
                    PrimitiveCount = atlasBuffer.Buffer.PrimitiveCount,
                    TextureSize = atlasBuffer.TextureSize
                });
            }
        }

        return new ChunkMesh(parts, _isLines);
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
                buffer = new AtlasBuffer(new BufferGenerator<VertexPositionBlockFace>(), textureReference.UvSize);
            
        }
        
        AddFaces(blockFace, i, j, maxI, maxJ, localBlockPosition, buffer.Buffer, textureReference);
    }

    private void AddFaces(BlockFace blockFace, int i, int j, int maxI, int maxJ, IntVector3 localPos, BufferGenerator<VertexPositionBlockFace> buffer, TextureAtlasReference textureReference)
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
            
        VertexPositionBlockFace ToVertex(Vector2 uv)
        {
            var position = faceTopRight + (cross * -(1 - uv.X) + up * -uv.Y) * faceSize;

            uv.X = 1 - uv.X; // invert x for now...
            uv *= size;

            //uv *= textureReference.UvSize;
            //uv += textureReference.Uv;

            return new VertexPositionBlockFace(position, uv, textureReference.Uv);
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