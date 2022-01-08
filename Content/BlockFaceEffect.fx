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
float2 TextureUv;
float2 TextureSize;

struct VSInput
{
    float4 Position : SV_Position;
    float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 PositionPS : SV_Position;
    float2 TexCoord   : TEXCOORD0;
};

VSOutput MainVS(VSInput vin)
{
    VSOutput vout;
    
    vout.PositionPS = mul(vin.Position, WorldViewProjection);

    vout.TexCoord = vin.TexCoord;

    return vout;
}

float4 MainPS(VSOutput pin) : SV_Target0
{
    float2 texCoord;
    texCoord.x = (pin.TexCoord.x % 1) * TextureSize.x + TextureUv.x;
    texCoord.y = (pin.TexCoord.y % 1) * TextureSize.y + TextureUv.y;
    
    return Texture.Sample(TextureSampler, texCoord);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
