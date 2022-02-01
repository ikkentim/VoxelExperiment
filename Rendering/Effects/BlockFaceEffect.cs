using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyGame.Rendering.Effects;

public class BlockFaceEffect : Effect, IEffectMatrices
{
    private readonly EffectParameter _lineColorParam;
    private readonly EffectParameter _textureParam;
    private readonly EffectParameter _textureSizeParam;
    private readonly EffectParameter _viewProjectionParam;
    private readonly EffectParameter _lightDirectionParam;
    private readonly EffectParameter _lightsViewParam;
    private readonly EffectParameter _worldParam;
    private readonly EffectParameter _shadowMapParam;
    private readonly EffectParameter _shadowMapSizeParam;

    private DirtyFlags _dirtyFlags;

    private bool _drawLines;  // technique
    private bool _isShadowMap; // technique
    private Color _lineColor;
    private Vector2 _textureSize;
    private Vector2 _shadowMapSize;
    private Vector3 _lightDirection;
    private Matrix _lightViewProjection;
    private Matrix _world;
    private Matrix _view;
    private Matrix _projection;

    public BlockFaceEffect(Effect cloneSource) : base(cloneSource)
    {
        _textureParam = Parameters["Texture"];
        _viewProjectionParam = Parameters["ViewProjection"];
        _textureSizeParam = Parameters["TextureSize"];
        _lineColorParam = Parameters["LineColor"];
        _lightDirectionParam = Parameters["LightDirection"];
        _shadowMapParam = Parameters["ShadowMap"];
        _lightsViewParam = Parameters["LightViewProj"];
        _worldParam = Parameters["World"];
        _shadowMapSizeParam = Parameters["ShadowMapSize"];

        if (cloneSource is BlockFaceEffect known)
        {
            Texture = known.Texture;
            ShadowMap = known.ShadowMap;

            _dirtyFlags = known._dirtyFlags;
            
            _drawLines = known._drawLines;
            _isShadowMap = known._isShadowMap;
            _lineColor = known._lineColor;
            _textureSize = known._textureSize;
            _lightDirection = known._lightDirection;
            _lightViewProjection = known._lightViewProjection;
            _world = known._world;
            _view = known._view;
            _projection = known._projection;
        }
    }
    
    public Texture2D Texture
    {
        get => _textureParam.GetValueTexture2D();
        set => _textureParam.SetValue(value);
    }

    public Texture2D? ShadowMap
    {
        get => _shadowMapParam?.GetValueTexture2D();
        set => _shadowMapParam?.SetValue(value);
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
    
    public Vector3 LightDirection
    {
        get => _lightDirection;
        set
        {
            _lightDirection = value;
            _dirtyFlags |= DirtyFlags.LightDirection;
        }
    }

    public Vector2 ShadowMapSize
    {
        get => _shadowMapSize;
        set
        {
            _shadowMapSize = value;
            _dirtyFlags |= DirtyFlags.ShadowMapSize;
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

    public bool IsShadowMap
    {
        get => _isShadowMap;
        set
        {
            _isShadowMap = value;
            _dirtyFlags |= DirtyFlags.Technique;
        }
    }

    public Matrix Projection
    {
        get => _projection;
        set
        {
            _projection = value;
            _dirtyFlags |= DirtyFlags.ViewProjection;
        }
    }
    
    public Matrix View
    {
        get => _view;
        set
        {
            _view = value;
            _dirtyFlags |= DirtyFlags.ViewProjection;
        }
    }

    public Matrix LightViewProjection
    {
        get => _lightViewProjection;
        set
        {
            _lightViewProjection = value;
            _dirtyFlags |= DirtyFlags.LightViewProj;
        }
    }

    public Matrix World
    {
        get => _world;
        set
        {
            _world = value;
            _dirtyFlags |= DirtyFlags.World;
        }
    }
    
    protected override void OnApply()
    {
        if ((_dirtyFlags & DirtyFlags.ViewProjection) != 0)
        {
            _viewProjectionParam.SetValue(View * Projection);
        }
        if ((_dirtyFlags & DirtyFlags.World) != 0)
        {
            _worldParam.SetValue(World);
        }
        
        if ((_dirtyFlags & DirtyFlags.TextureSize) != 0)
        {
            _textureSizeParam.SetValue(_textureSize);
        }

        if ((_dirtyFlags & DirtyFlags.LightDirection) != 0)
        {
            _lightDirectionParam.SetValue(_lightDirection);
        }

        if ((_dirtyFlags & DirtyFlags.LineColor) != 0)
        {
            _lineColorParam.SetValue(_lineColor.ToVector4());
        }
        
        if ((_dirtyFlags & DirtyFlags.LightViewProj) != 0)
        {
            _lightsViewParam.SetValue(_lightViewProjection);
        }

        if ((_dirtyFlags & DirtyFlags.ShadowMapSize) != 0)
        {
            _shadowMapSizeParam.SetValue(_shadowMapSize);
        }

        if ((_dirtyFlags & DirtyFlags.Technique) != 0)
        {
            CurrentTechnique = _isShadowMap
                ? Techniques[2]
                : _drawLines
                    ? Techniques[1]
                    : Techniques[0];
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
        ViewProjection,
        World,
        LightViewProj,
        TextureSize,
        LineColor,
        Technique,
        LightDirection,
        ShadowMapSize
    }
}