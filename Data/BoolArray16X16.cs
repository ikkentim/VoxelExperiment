using System;

namespace MyGame.Data;

public struct BoolArray16X16
{
    private ulong _a;
    private ulong _b;
    private ulong _c;
    private ulong _d;

    public bool this[int index]
    {
        get => Get(index);
        set => Set(index, value);
    }

    public bool this[int x, int y]
    {
        get => Get(x + y * 16);
        set => Set(x + y * 16, value);
    }

    public bool Get(int index)
    {
        if (index is < 0 or >= 256)
            throw new ArgumentOutOfRangeException(nameof(index));

        var rem = index % 64;
        return (index / 64) switch
        {
            0 => ((_a >> rem) & 1) == 1,
            1 => ((_b >> rem) & 1) == 1,
            2 => ((_c >> rem) & 1) == 1,
            _ => ((_d >> rem) & 1) == 1
        };
    }

    public void Set(int index, bool value)
    {
        if (index < 0 || index >= 256)
            throw new ArgumentOutOfRangeException(nameof(index));

        var rem = index % 64;
        var mask = (ulong)1 << rem;

        switch (index / 64)
        {
            case 0:
                _a = (_a & ~mask) | (value ? mask : 0);
                break;
            case 1:
                _b = (_b & ~mask) | (value ? mask : 0);
                break;
            case 2:
                _c = (_c & ~mask) | (value ? mask : 0);
                break;
            default:
                _d = (_d & ~mask) | (value ? mask : 0);
                break;
        }
    }
}