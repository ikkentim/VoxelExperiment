﻿using System;
using System.Collections.Generic;
using MyGame.Data;

namespace MyGame.World.Blocks;

public class AirBlock : Block
{
    public static AirBlock Instance { get; } = new();

    public override string Name => "air";

    public override bool IsOpaque => false;

    public override TextureReference GetTexture(BlockFace face) => new();

    public override IEnumerable<TextureReference> GetTextures() => Array.Empty<TextureReference>();
}