using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyGame.Rendering.Effects;

public class BlockFaceEffect : Effect, IEffectMatrices
{
    private readonly EffectParameter _lineColorParam;


    private readonly EffectParameter _textureParam;
    private readonly EffectParameter _textureSizeParam;
    private readonly EffectParameter _worldViewProjectionParam;
    private DirtyFlags _dirtyFlags;

    private bool _drawLines;
    private Color _lineColor;
    private Matrix _projection;
    private Vector2 _textureSize;
    private Matrix _view;
    private Matrix _world;

    public BlockFaceEffect(Effect cloneSource) : base(cloneSource)
    {
        _textureParam = Parameters["Texture"];
        _worldViewProjectionParam = Parameters["WorldViewProjection"];
        _textureSizeParam = Parameters["TextureSize"];
        _lineColorParam = Parameters["LineColor"];

        if (cloneSource is BlockFaceEffect known)
        {
            _projection = known._projection;
            _view = known._view;
            _world = known._world;
            _textureSize = known._textureSize;
            _lineColor = known._lineColor;
            _dirtyFlags = known._dirtyFlags;
        }
    }

    public Texture2D Texture
    {
        get => _textureParam.GetValueTexture2D();
        set => _textureParam.SetValue(value);
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

    public Color LineColor
    {
        get => _lineColor;
        set
        {
            _lineColor = value;
            _dirtyFlags |= DirtyFlags.LineColor;
        }
    }

    public bool DrawLines
    {
        get => _drawLines;
        set
        {
            _drawLines = value;
            _dirtyFlags |= DirtyFlags.Technique;
        }
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

    protected override void OnApply()
    {
        if ((_dirtyFlags & DirtyFlags.WorldViewProjection) != 0)
        {
            _worldViewProjectionParam.SetValue(World * View * Projection);
        }

        if ((_dirtyFlags & DirtyFlags.TextureSize) != 0)
        {
            _textureSizeParam.SetValue(_textureSize);
        }

        if ((_dirtyFlags & DirtyFlags.LineColor) != 0)
        {
            _lineColorParam.SetValue(_lineColor.ToVector4());
        }

        if ((_dirtyFlags & DirtyFlags.Technique) != 0)
        {
            CurrentTechnique = _drawLines ? Techniques[1] : Techniques[0];
        }

        _dirtyFlags = DirtyFlags.None;


        base.OnApply();
    }

    public override Effect Clone()
    {
        return new BlockFaceEffect(this);
    }

    [Flags]
    private enum DirtyFlags
    {
        None,
        WorldViewProjection,
        TextureSize,
        LineColor,
        Technique
    }
}