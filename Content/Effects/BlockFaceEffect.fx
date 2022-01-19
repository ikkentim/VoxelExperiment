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
float3 LightDirection;

struct VSInput
{
    float4 Position : SV_Position;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
    float2 TextureBase : TEXCOORD1;
};

struct VSOutput
{
    float4 PositionPS : SV_Position;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
    float2 TextureBase : TEXCOORD1;
};

VSOutput MainVS(VSInput vin)
{
    VSOutput vout;
    
    vout.PositionPS = mul(vin.Position, WorldViewProjection);
    
    vout.TexCoord = vin.TexCoord;
    vout.TextureBase = vin.TextureBase;
    vout.Normal = vin.Normal;

    return vout;
}

float4 MainLineVS(float4 pos : SV_Position) : SV_Position
{
    return mul(pos, WorldViewProjection);
}

float4 MainPS(VSOutput pin) : SV_Target0
{
    float2 texCoord;
    texCoord.x = (pin.TexCoord.x % 1) * TextureSize.x + pin.TextureBase.x;
    texCoord.y = (pin.TexCoord.y % 1) * TextureSize.y + pin.TextureBase.y;
    
    float3 normal = normalize(pin.Normal);
    
    float4 diffuse = float4(0.4, 0.4, 0.4, 1); // ambientLightColor
    float diffuseLightColor = float4(1.0, 0.9, 0.8, 1);

    float NdotL = saturate(dot(normal, LightDirection));
    diffuse += NdotL * diffuseLightColor;

    float4 col = Texture.Sample(TextureSampler, texCoord) * diffuse;

    col.a = 1;

    return col;
}

float4 MainLinePS(float4 pos : SV_Position) : SV_Target0
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