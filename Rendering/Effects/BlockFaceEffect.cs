using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyGame.Components;

public class BlockFaceEffect : Effect, IEffectMatrices
{
    private Matrix _projection;
    private Matrix _view;
    private Matrix _world;
    private Vector2 _textureUv;
    private Vector2 _textureSize;
    private DirtyFlags _dirtyFlags;


    private readonly EffectParameter _textureParam;
    private readonly EffectParameter _worldViewProjectionParam;
    private readonly EffectParameter _textureUvParam;
    private readonly EffectParameter _textureSizeParam;

    public BlockFaceEffect(Effect cloneSource) : base(cloneSource)
    {
        _textureParam = Parameters["Texture"];
        _worldViewProjectionParam = Parameters["WorldViewProjection"];
        _textureUvParam = Parameters["TextureUv"];
        _textureSizeParam = Parameters["TextureSize"];

        if (cloneSource is BlockFaceEffect known)
        {
            _projection = known._projection;
            _view = known._view;
            _world = known._world;
            _textureUv = known._textureUv;
            _textureSize = known._textureSize;
            _dirtyFlags = known._dirtyFlags;
        }
    }

    public Texture2D Texture
    {
        get => _textureParam!.GetValueTexture2D();
        set => _textureParam!.SetValue(value);
    }

    public Matrix Projection
    {
        get => _projection;
        set
        {
            _projection = value;
            _dirtyFlags |= DirtyFlags.WorldViewProjection;
        }
    }

    public Matrix View
    {
        get => _view;
        set
        {
            _view = value;
            _dirtyFlags |= DirtyFlags.WorldViewProjection;
        }
    }

    public Matrix World
    {
        get => _world;
        set
        {
            _world = value;
            _dirtyFlags |= DirtyFlags.WorldViewProjection;
        }
    }

    public Vector2 TextureUv
    {
        get => _textureUv;
        set
        {
            _textureUv = value; 
            _dirtyFlags |= DirtyFlags.TextureUv;
        }
    }

    public Vector2 TextureSize
    {
        get => _textureSize;
        set
        {
            _textureSize = value; 
            _dirtyFlags |= DirtyFlags.TextureSize;
        }
    }
    
    protected override void OnApply()
    {
        if ((_dirtyFlags & DirtyFlags.WorldViewProjection) != 0)
        {
            _worldViewProjectionParam.SetValue(World * View * Projection);
        }
        if ((_dirtyFlags & DirtyFlags.TextureUv) != 0)
        {
            _textureUvParam.SetValue(_textureUv);
        }
        if ((_dirtyFlags & DirtyFlags.TextureSize) != 0)
        {
            _textureSizeParam.SetValue(_textureSize);
        }

        _dirtyFlags = DirtyFlags.None;

        CurrentTechnique = Techniques[0];

        base.OnApply();
    }

    [Flags]
    private enum DirtyFlags
    {
        None,
        WorldViewProjection,
        TextureUv,
        TextureSize,
    }
}