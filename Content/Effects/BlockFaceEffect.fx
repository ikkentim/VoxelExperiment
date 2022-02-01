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

Texture2D<float4> ShadowMap : register(t1);
sampler ShadowMapSampler : register(s1) =
sampler_state
{
    Texture = <ShadowMap>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

matrix World;
matrix ViewProjection;
float2 TextureSize;
float4 LineColor;
float3 LightDirection;
matrix LightViewProj;
float2 ShadowMapSize;

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
    float4 WorldPos : TEXCOORD2;
};

struct VSShadowInput
{
    float4 Position : SV_Position;
};

struct VSShadowOutput
{
    float4 Position : SV_Position;
    float Depth : TEXCOORD0;
};

VSOutput MainVS(VSInput inp)
{
    VSOutput outp;
    
    outp.PositionPS = mul(inp.Position, mul(World, ViewProjection));
    
    outp.TexCoord = inp.TexCoord;
    outp.TextureBase = inp.TextureBase;
    outp.Normal = normalize(mul(inp.Normal, (float3x4) World));
    outp.WorldPos = mul(inp.Position, World);

    return outp;
}

VSShadowOutput MainShadowVS(float4 position : SV_Position)
{
    VSShadowOutput outp;
    
    outp.Position = mul(position, mul(World, LightViewProj));
    outp.Depth = outp.Position.z / outp.Position.w;

    return outp;
}

float4 MainShadowPS(VSShadowOutput inp) : SV_Target0
{
    return float4(inp.Depth, 0, 0, 1);
}

float4 MainLineVS(float4 pos : SV_Position) : SV_Position
{
    return mul(pos, mul(World, ViewProjection));
}

float4 MainPS(VSOutput inp) : SV_Target0
{
    // sample the texture
    float2 texCoord = (inp.TexCoord % 1) * TextureSize + inp.TextureBase;
    float4 diffuseColor = Texture.Sample(TextureSampler, texCoord);

    // sample the shadow map
    float4 lightingPosition = mul(inp.WorldPos, LightViewProj);
    float2 ShadowTexCoord = 0.5 * lightingPosition.xy / lightingPosition.w + float2(0.5, 0.5);
    ShadowTexCoord.y = 1.0f - ShadowTexCoord.y;

    float2 TexelSize = float2(1, 1) / ShadowMapSize;

    float DepthBias = 0.0005;
    float4 AmbientColor = float4(0.15, 0.15, 0.15, 0);
    float shadowStrength = 0.5;



	
    float vertexDepth = (lightingPosition.z / lightingPosition.w) - DepthBias;
    float diffuseIntensity = max(0.2, saturate(dot(-LightDirection, inp.Normal)));
	
    float shadow = 0;

    for (int x = -1; x <= 1; x++)
    {
        for (int y = -1; y <= 1; y++)
        {
            float shadowDepth = ShadowMap.Sample(ShadowMapSampler, ShadowTexCoord + float2(x, y) * TexelSize).r;
            if (shadowDepth < vertexDepth)
            {
                shadow += shadowStrength;
            }
        }
    }
    
    shadow /= 9;




    float4 diffuse = diffuseIntensity * diffuseColor + AmbientColor;
	
    diffuse *= float4(1 - shadow, 1 - shadow, 1 - shadow, 0);
  

    diffuse.a = 1;
    return diffuse;
    
}

float4 MainLinePS(VSOutput inp) : SV_Target0
{
    return LineColor;
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

technique BlockFaceShadowMap
{
    pass Tex
    {
        VertexShader = compile VS_SHADERMODEL MainShadowVS();
        PixelShader = compile PS_SHADERMODEL MainShadowPS();
    }
};
