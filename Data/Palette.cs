using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MyGame.Data;

public class Palette<T> where T : struct, IEquatable<T>
{
    private readonly byte _minBits;
    private readonly List<Container> _states = new();
    private int _usedSlots;
    private byte? _bitCache;

    public byte BitsRequiredForPaletteIndices => _bitCache ??= CalcBits();

    public Palette(byte minBits)
    {
        _minBits = minBits;
    }

    public int Add(T value)
    {
        // default value is reserved and always at slot 0
        if (value.Equals(default))
        {
            return 0;
        }

        // find existing slot in list
        var empty = -1;
        for (var i = 0; i < _states.Count; i++)
        {
            var container = _states[i];
            if (container.State.Equals(value))
            {
                if (container.Count++ == 0)
                {
                    _usedSlots++;
                }

                _states[i] = container;

                return i + 1;
            }

            if (container.Count == 0)
            {
                empty = i;
            }
        }

        // fill empty slot in list
        if (empty != -1)
        {
            var container = _states[empty];
            container.State = value;
            container.Count = 1;
            _states[empty] = container;

            _usedSlots++;
            return empty + 1;
            
        }
        
        // add new slot
        _states.Add(new Container
        {
            Count = 1,
            State = value
        });

        _bitCache = null;

        return _states.Count;
    }

    public T Get(int index)
    {
        if (index == 0)
        {
            return default;
        }

        if (index < 0 || index > _states.Count)
        {
            return default;
        }

        var container = _states[index - 1];

        return container.Count == 0 ? default : container.State;
    }
    
    public void Remove(int index)
    {
        index--;

        if (index < 0 || index >= _states.Count)
        {
            return;
        }

        var container = _states[index];

        if (container.Count == 0)
        {
            return;
        }
        
        if (--container.Count == 0)
        {
            _usedSlots--;
        }

        _states[index] = container;
    }
    
    private byte CalcBits()
    {
        // add slot because slot 0 in palette is reserved for the default value
        var number = _usedSlots + 1;
        byte bits = 0;
        while (number != 0)
        {
            number >>= 1;
            bits++;
        }

        return Math.Max(_minBits, bits);
    }
    
    [DebuggerDisplay("State = {State}, Count = {Count}")]
    private struct Container
    {
        public T State;
        public int Count;
    }
}