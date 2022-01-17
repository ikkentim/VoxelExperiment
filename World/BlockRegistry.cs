using System;
using System.Collections.Generic;

namespace MyGame.World;

public class BlockRegistry
{
    private readonly Dictionary<string, Block> _blockTypes = new();
    private bool _isLocked;

    public bool IsLocked => _isLocked;

    public void Register(Block block)
    {
        if (_isLocked)
        {
            throw new InvalidOperationException("Registry is locked");
        }

        var name = block.Name;

        if (_blockTypes.ContainsKey(name))
        {
            throw new InvalidOperationException("Duplicate block with same name: " + name);
        }

        _blockTypes.Add(name, block);
    }

    public void Lock()
    {
        _isLocked = true;
    }

    public Block GetBlock(string name)
    {
        return _blockTypes.TryGetValue(name, out var block)
            ? block
            : throw new InvalidOperationException("block unknown " + name);
    }

    public IEnumerable<Block> GetBlockTypes()
    {
        return _blockTypes.Values;
    }
}