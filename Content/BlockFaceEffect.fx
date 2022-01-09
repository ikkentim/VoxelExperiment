#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D<float4> Texture : register(t0);
sampler TextureSampler : register(s0);

matrix WorldViewProjection;
float2 TextureSize;
float4 LineColor;

struct VSInput
{
    float4 Position : SV_Position;
    float2 TexCoord : TEXCOORD0;
    float2 TextureBase : TEXCOORD1;
};

struct VSOutput
{
    float4 PositionPS : SV_Position;
    float2 TexCoord : TEXCOORD0;
    float2 TextureBase : TEXCOORD1;
};

struct VSLineInput
{
    float4 Position : SV_Position;
};

struct VSLineOutput
{
    float4 PositionPS : SV_Position;
};

VSOutput MainVS(VSInput vin)
{
    VSOutput vout;
    
    vout.PositionPS = mul(vin.Position, WorldViewProjection);
    
    vout.TexCoord = vin.TexCoord;
    vout.TextureBase = vin.TextureBase;

    return vout;
}

VSLineOutput MainLineVS(VSLineInput vin)
{
    VSLineOutput vout;

    vout.PositionPS = mul(vin.Position, WorldViewProjection);

    return vout;
}

float4 MainPS(VSOutput pin) : SV_Target0
{
    float2 texCoord;
    texCoord.x = (pin.TexCoord.x % 1) * TextureSize.x + pin.TextureBase.x;
    texCoord.y = (pin.TexCoord.y % 1) * TextureSize.y + pin.TextureBase.y;
    
    return Texture.Sample(TextureSampler, texCoord);
}

float4 MainLinePS(VSLineOutput pin) : SV_TARGET0
{
    return LineColor;
}

technique BlockFaceDrawing
{
	pass Tex
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};

technique BlockFaceLines
{
    pass Lines
    {
        VertexShader = compile VS_SHADERMODEL MainLineVS();
        PixelShader = compile PS_SHADERMODEL MainLinePS();
    }
}