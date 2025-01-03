﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace MyGame.Rendering;

public class BufferGenerator<T> : IDisposable where T : struct
{
    private readonly List<T> _vertices = new();
    private IndexBuffer? _indexBuffer;
    private IndexElementSize _indexSize;
    private List<int>? _intList;

    private int _primitiveCount;
    private List<short>? _shortList = new();
    private VertexBuffer? _vertexBuffer;

    public int PrimitiveCount => _primitiveCount;

    public bool IsEmpty => _vertices.Count == 0;

    public void Dispose()
    {
        _indexBuffer?.Dispose();
        _vertexBuffer?.Dispose();

        GC.SuppressFinalize(this);
    }


    public void AddFace(T a, T b, T c, T d)
    {
        var start = _vertices.Count;
        _vertices.Add(a);
        _vertices.Add(b);
        _vertices.Add(c);
        _vertices.Add(d);

        _primitiveCount += 2;

        // a - b
        // |   |
        // c - d
        if (_shortList == null || _shortList.Count >= short.MaxValue - 6)
        {
            if (_shortList != null)
            {
                _intList = new List<int>(short.MaxValue + 10);
                _intList.AddRange(_shortList.Select(x => (int)x));
                _shortList = null;
            }

            _intList!.Add(start + 2);
            _intList.Add(start + 1);
            _intList.Add(start + 0);
            _intList.Add(start + 3);
            _intList.Add(start + 1);
            _intList.Add(start + 2);
        }
        else
        {
            _shortList.Add((short)(start + 2));
            _shortList.Add((short)(start + 1));
            _shortList.Add((short)(start + 0));
            _shortList.Add((short)(start + 3));
            _shortList.Add((short)(start + 1));
            _shortList.Add((short)(start + 2));
        }
    }

    public void AddFaceLines(T a, T b, T c, T d)
    {
        var start = _vertices.Count;
        _vertices.Add(a);
        _vertices.Add(b);
        _vertices.Add(c);
        _vertices.Add(d);

        _primitiveCount += 5;

        // a - b
        // |   |
        // c - d
        if (_shortList == null || _shortList.Count >= short.MaxValue - 6)
        {
            if (_shortList != null)
            {
                _intList = new List<int>(short.MaxValue + 10);
                _intList.AddRange(_shortList.Select(x => (int)x));
                _shortList = null;
            }

            _intList!.Add(start + 2);
            _intList.Add(start + 1);
            _intList.Add(start + 1);
            _intList.Add(start + 0);
            _intList.Add(start + 0);
            _intList.Add(start + 2);
            _intList.Add(start + 2);
            _intList.Add(start + 3);
            _intList.Add(start + 3);
            _intList.Add(start + 1);
        }
        else
        {
            _shortList.Add((short)(start + 2));
            _shortList.Add((short)(start + 1));
            _shortList.Add((short)(start + 1));
            _shortList.Add((short)(start + 0));
            _shortList.Add((short)(start + 0));
            _shortList.Add((short)(start + 2));
            _shortList.Add((short)(start + 2));
            _shortList.Add((short)(start + 3));
            _shortList.Add((short)(start + 3));
            _shortList.Add((short)(start + 1));
        }
    }

    public (IndexBuffer, VertexBuffer) GetBuffers(GraphicsDevice graphicsDevice)
    {
        if (_vertexBuffer == null)
        {
            _vertexBuffer = new VertexBuffer(graphicsDevice, typeof(T), _vertices.Count, BufferUsage.WriteOnly);
        }
        else if (_vertexBuffer.VertexCount < _vertices.Count)
        {
            // resize
            _vertexBuffer.Dispose();
            _vertexBuffer = new VertexBuffer(graphicsDevice, typeof(T), _vertices.Count, BufferUsage.WriteOnly);
        }

        var indexCount = _shortList?.Count ?? _intList!.Count;
        var indexSize = _shortList == null ? IndexElementSize.ThirtyTwoBits : IndexElementSize.SixteenBits;
        if (_indexBuffer == null)
        {
            _indexBuffer = new IndexBuffer(graphicsDevice, indexSize, indexCount, BufferUsage.WriteOnly);
            _indexSize = indexSize;
        }
        else if (_indexSize != indexSize)
        {
            _indexBuffer.Dispose();
            _indexBuffer = new IndexBuffer(graphicsDevice, indexSize, indexCount, BufferUsage.WriteOnly);
            _indexSize = indexSize;
        }
        else if (_indexBuffer.IndexCount < indexCount)
        {
            _indexBuffer.Dispose();
            _indexBuffer = new IndexBuffer(graphicsDevice, indexSize, indexCount, BufferUsage.WriteOnly);
        }

        // TODO: Booh, copying...

        var vertices = _vertices.ToArray();
        _vertexBuffer.SetData(vertices);

        if (_shortList != null)
        {
            var indices = _shortList.ToArray();
            _indexBuffer.SetData(indices);
        }
        else
        {
            var indices = _intList!.ToArray();
            _indexBuffer.SetData(indices);
        }

        return (_indexBuffer, _vertexBuffer);
    }

    public void Clear()
    {
        _vertices.Clear();
        _intList?.Clear();
        _shortList?.Clear();

        _primitiveCount = 0;
    }
}

public class BufferGeneratorV2<T> where T : struct
{
    private readonly List<T> _vertices = new();
    private List<ushort> _indices = new();

    private int _primitiveCount;

    public int PrimitiveCount => _primitiveCount;
    
    public void AddFace(T a, T b, T c, T d)
    {
        var start = _vertices.Count;
        _vertices.Add(a);
        _vertices.Add(b);
        _vertices.Add(c);
        _vertices.Add(d);

        _primitiveCount += 2;

        // a - b
        // |   |
        // c - d
        _indices.Add((ushort)(start + 2));
        _indices.Add((ushort)(start + 1));
        _indices.Add((ushort)(start + 0));
        _indices.Add((ushort)(start + 3));
        _indices.Add((ushort)(start + 1));
        _indices.Add((ushort)(start + 2));
    }

    public void AddFaceLines(T a, T b, T c, T d)
    {
        var start = _vertices.Count;
        _vertices.Add(a);
        _vertices.Add(b);
        _vertices.Add(c);
        _vertices.Add(d);

        _primitiveCount += 5;

        // a - b
        // |   |
        // c - d
        _indices.Add((ushort)(start + 2));
        _indices.Add((ushort)(start + 1));
        _indices.Add((ushort)(start + 1));
        _indices.Add((ushort)(start + 0));
        _indices.Add((ushort)(start + 0));
        _indices.Add((ushort)(start + 2));
        _indices.Add((ushort)(start + 2));
        _indices.Add((ushort)(start + 3));
        _indices.Add((ushort)(start + 3));
        _indices.Add((ushort)(start + 1));
    }

    public (IndexBuffer, VertexBuffer, int) CreateBuffers(GraphicsDevice graphicsDevice)
    {
        var indexCount = _indices.Count;

        var vertexBuffer = new VertexBuffer(graphicsDevice, typeof(T), _vertices.Count, BufferUsage.WriteOnly);
        var indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indexCount, BufferUsage.WriteOnly);
        
        // TODO: Booh, copying...
        var vertices = _vertices.ToArray();
        vertexBuffer.SetData(vertices);
        
        var indices = _indices.ToArray();
        indexBuffer.SetData(indices);
  
        return (indexBuffer, vertexBuffer, _primitiveCount);
    }

    public void Clear()
    {
        _vertices.Clear();
        _indices?.Clear();

        _primitiveCount = 0;
    }
}