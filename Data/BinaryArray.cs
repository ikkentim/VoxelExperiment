using System;
using System.Buffers;
using System.Windows.Forms;

namespace MyGame.World;

public class BinaryArray : IDisposable
{
    private static readonly ArrayPool<ulong> ArrayPool = ArrayPool<ulong>.Create();

    private int _mask;
    private ulong[] _data;
    private bool _disposed;
    private byte _bitsPerValue;

    public BinaryArray(byte bitsPerValue, int length)
    {
        if (bitsPerValue is < 1 or > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(bitsPerValue));
        }

        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        _bitsPerValue = bitsPerValue;
        _mask = GetMask(bitsPerValue);
        Length = length;
        
        var backingLength = GetBackingLength(bitsPerValue);
        _data = ArrayPool.Rent(backingLength);
        Array.Clear(_data);
    }

    public byte BitsPerValue
    {
        get => _bitsPerValue;
        set
        {
            if (value is < 1 or > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            ResizeArray(value);
        }
    }

    public int Length { get; }

    public int this[int index]
    {
        get => Get(index);
        set => Set(index, value);
    }

    public int Get(int index)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(BinaryArray));

        if(index < 0 || index >= Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        var bitStart = BitsPerValue * index;
        var bitStartInValue1 = bitStart % 64;
        var dataIndex = bitStart / 64;
        var bitsAvailableInValue1 = 64 - bitStartInValue1;

        var value1 = _data[dataIndex];
        value1 >>= bitStartInValue1;

        if (bitsAvailableInValue1 < BitsPerValue)
        {
            var value2 = _data[dataIndex + 1];
            value2 <<= bitsAvailableInValue1;
            return (int)(value2 | value1) & _mask;
        }

        return (int)value1 & _mask;
    }

    public void Set(int index, int value)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(BinaryArray));

        if(index < 0 || index >= Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if(value > _mask || value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }
        
        SetInner(_data, _bitsPerValue, (ulong)_mask, index, value);
    }

    private void ResizeArray(byte bitsPerValue)
    {
        var newBackingLength = GetBackingLength(bitsPerValue);
        var newArray = ArrayPool.Rent(newBackingLength);
        var newMask = GetMask(bitsPerValue);
        Array.Clear(newArray);

        for (var i = 0; i < Length; i++)
        {
            SetInner(newArray, bitsPerValue, (ulong)newMask, i, Get(i));
        }

        ArrayPool.Return(_data);
        _bitsPerValue = bitsPerValue;
        _mask = newMask;
        _data = newArray;
    }

    private static void SetInner(ulong[] array, byte bitsPerValue, ulong mask, int index, int value)
    {
        var bitStart = bitsPerValue * index;
        var bitStartInValue1 = bitStart % 64;
        var dataIndex = bitStart / 64;
        var bitsAvailableInValue1 = 64 - bitStartInValue1;

        array[dataIndex] = (array[dataIndex] & ~(mask << bitStartInValue1)) | ((ulong)value << bitStartInValue1);

        if (bitsAvailableInValue1 < bitsPerValue)
        {
            array[dataIndex + 1] = (array[dataIndex + 1] & ~(mask >> bitsAvailableInValue1)) | ((ulong)value >> bitsAvailableInValue1);
        }
    }
    
    private static int GetMask(byte bitsPerValue)
    {
        var mask = 0;
        for (var i = 0; i < bitsPerValue; i++)
        {
            mask = (mask << 1) | 1;
        }
        return mask;
    }

    private int GetBackingLength(byte bitsPerValue)
    {
        var bits = (ulong)Length * bitsPerValue;
        return (int)(bits / 64) + (bits % 64 == 0 ? 0 : 1);
    }

    public void Dispose()
    {
        ArrayPool.Return(_data);
        _data = null!;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}