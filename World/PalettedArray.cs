using System;
using MyGame.Data;

namespace MyGame.World;

public class PalettedArray<T> where T : struct, IEquatable<T>
{
    private readonly Palette<T> _palette = new(4);
    private BinaryArray _binaryArray;

    public PalettedArray(int length)
    {
        _binaryArray = new BinaryArray(4, length);
    }

    public T Replace(int index, T value)
    {
        var oldPos = _binaryArray[index];
        var result = _palette.Get(oldPos);

        if (oldPos != 0)
        {
            _palette.Remove(oldPos);
        }

        var paletteIndex = _palette.Add(value);
        
        if (_palette.RequiredBits > _binaryArray.BitsPerValue)
        {
            // Resize structure
            var newArray = new BinaryArray(_palette.RequiredBits, _binaryArray.Length);

            for (var i = 0; i < _binaryArray.Length; i++)
            {
                newArray[i] = _binaryArray[i];
            }

            _binaryArray.Dispose();
            _binaryArray = newArray;
        }

        _binaryArray[index] = paletteIndex;

        return result;
    }

    public T Get(int index)
    {
        var position = _binaryArray[index];
        return _palette.Get(position);
    }

    public void Optimize()
    {
        // todo
    }
}