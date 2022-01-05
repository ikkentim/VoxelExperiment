﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyGame.Helpers;

namespace MyGame.World.Rendering
{
    public class WorldChunkMeshGenerator
    {
        private readonly WorldChunk _chunk;
        private readonly TextureProvider _textureProvider;
        private readonly bool _isLines;
        private readonly Dictionary<Texture2D, BufferGenerator<VertexPositionTexture>> _bufferPerAtlas = new();

        public WorldChunkMeshGenerator(WorldChunk chunk, TextureProvider textureProvider, bool isLines)
        {
            _chunk = chunk;
            _textureProvider = textureProvider;
            _isLines = isLines;

            Debug.Assert(WorldChunk.ChunkSize == 16); // required for using BoolArray16X16
        }

        public ChunkMesh Create(GraphicsDevice graphicsDevice)
        {
            foreach (var kv in _bufferPerAtlas)
            {
                kv.Value.Clear();
            }
            
            for (var depth = 0; depth < WorldChunk.ChunkSize; depth++)
                foreach (var face in Faces.AllFaces)
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

            return new ChunkMesh(parts.ToArray(), _isLines);
        }

        private void GreedyMesh(Face face, int depth)
        {
            var visited = new BoolArray16X16();
            
            for (var j = 0; j < WorldChunk.ChunkSize; j++)
            for (var i = 0; i < WorldChunk.ChunkSize; i++)
            {
                if (visited[i, j])
                    continue;

                GreedyMeshFromPoint(face, depth, i, j, ref visited);
            }
        }

        private void GreedyMeshFromPoint(Face face, int depth, int i, int j, ref BoolArray16X16 visited)
        {
            var localBlockPosition = GetPosition(face, depth, i, j);
            var source = _chunk.GetBlock(localBlockPosition);

            if ((source.VisibleFaces & face) == Face.None)
            {
                // Face not visible - skip
                visited[i, j] = true;
                return;
            }
            
            // The texture we'll be merging in the current routine
            var sourceTexture = source.Kind!.GetTexture().Name;

            // Expand covered area towards i+
            var maxI = i;
            for (var i2 = i + 1; i2 < WorldChunk.ChunkSize; i2++)
            {
                if (visited[i2, j])
                { 
                    // face was already visited somehow (shouldn't happen?)
                    break;
                }

                var block = _chunk.GetBlock(GetPosition(face, depth, i2, j));

                if ((block.VisibleFaces & face) == Face.None)
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
            for (var j2 = j + 1; j2 < WorldChunk.ChunkSize; j2++)
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
                    
                    var block = _chunk.GetBlock(GetPosition(face, depth, i2, j2));
                    
                    if ((block.VisibleFaces & face) == Face.None)
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
            AddFaces(face, depth, i, j, maxI, maxJ, localBlockPosition, buffer);
        }

        private void AddFaces(Face face, int depth, int i, int j, int maxI, int maxJ, IntVector3 vertexOrigin, BufferGenerator<VertexPositionTexture> buffer)
        {
            var lenI = maxI - i + 1;
            var lenJ = maxJ - j + 1;
            
            var faceSize = GetPosition(face, 0, lenI, lenJ);
            
            Vector3 Vx(int x, int y)
            {
                return GetFaceVertex(vertexOrigin, face, faceSize, new Vector2(x, y));
            }

            Vector2 Uv(int x, int y)
            {
                var mul = face is Face.North or Face.South ? new Vector2(lenI, lenJ) : new Vector2(lenJ, lenI);
                return new Vector2(1-x, y) * mul;
            }


            Debug.WriteLine(
                $"Adding face {Vx(0, 0)},{Vx(1, 0)},{Vx(0, 1)},{Vx(1, 1)} - normal {Faces.GetNormal(face)} ({face}) depth {depth} i {i}-{maxI} j {j}-{maxJ}");

            if (_isLines)
            {
                buffer.AddFaceLines(
                    new VertexPositionTexture(Vx(0, 0), Uv(0, 0)),
                    new VertexPositionTexture(Vx(1, 0), Uv(1, 0)),
                    new VertexPositionTexture(Vx(0, 1), Uv(0, 1)),
                    new VertexPositionTexture(Vx(1, 1), Uv(1, 1))
                );
            }
            else
            {
                buffer.AddFace(
                    new VertexPositionTexture(Vx(0, 0), Uv(0, 0)),
                    new VertexPositionTexture(Vx(1, 0), Uv(1, 0)),
                    new VertexPositionTexture(Vx(0, 1), Uv(0, 1)),
                    new VertexPositionTexture(Vx(1, 1), Uv(1, 1))
                );
            }
        }

        private static Vector3 GetFaceVertex(IntVector3 blockPosition, Face face, Vector3 faceSize, Vector2 facePosition)
        {
            Vector3 position = blockPosition;
            var normal = Faces.GetNormal(face);
            var up = Faces.GetUp(face);

            if (Faces.IsPositive(face))
            {
                position += normal;
            }

            var cross = Vector3.Cross(normal, up);
            var absCross = new Vector3(
                MathF.Abs(cross.X),
                MathF.Abs(cross.Y),
                MathF.Abs(cross.Z)
            );


            // Get center
            position += (absCross + up) * (faceSize / 2);

            // Get top-right
            position += (cross + up) * (faceSize / 2);

            // Get desired position on face
            position += (cross * -(1 - facePosition.X) + up * -facePosition.Y) * faceSize;

            return position;
        }

        private BufferGenerator<VertexPositionTexture> GetBuffer(string textureName)
        {
            var atlas = _textureProvider.GetTexture(textureName);
            return _bufferPerAtlas.GetOrAdd(atlas);
        }
        
        private static IntVector3 GetPosition(Face face, int depth, int i, int j)
        {
            var normal = Faces.GetNormal(face);
            var pos = normal * depth;

            if ((face & Faces.PositiveFaces) == Face.None)
                pos = -pos;
            
            if (normal.X != 0)
            {
                pos.Y = i;
                pos.Z = j;
            }
            else if (normal.Y != 0)
            {
                pos.X = j;
                pos.Z = i;
            }
            else
            {
                pos.X = i;
                pos.Y = j;
            }

            return pos;
        }
    }
}
