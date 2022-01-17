using System;
using System.Collections;
using System.Collections.Generic;

namespace MyGame.Data;

public class PaletteBackedArray<T> : IEnumerable<T> where T : struct, IEquatable<T>
{
    private readonly Palette<T> _palette = new(4);
    private readonly BinaryArray _data;

    public PaletteBackedArray(int length)
    {
        _data = new BinaryArray(4, length);
    }

    public T Replace(int index, T value)
    {
        var oldPos = _data[index];
        var result = _palette.Get(oldPos);

        if (oldPos != 0)
        {
            _palette.Remove(oldPos);
        }

        var paletteIndex = _palette.Add(value);
        
        
        if (_palette.BitsRequiredForPaletteIndices > _data.BitsPerValue)
        {
            _data.BitsPerValue = _palette.BitsRequiredForPaletteIndices;
        }

        _data[index] = paletteIndex;

        return result;
    }

    public T Get(int index)
    {
        var position = _data[index];
        return _palette.Get(position);
    }

    public void Optimize()
    {
        // todo
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var index in _data)
        {
            yield return Get(index);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}