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

Texture2D<float4> DepthBuffer : register(t1); //todo;rename to ShadowMap
sampler DepthBufferSampler : register(s1);

matrix World;
matrix WorldViewProjection;
float2 TextureSize;
float4 LineColor;
float3 LightDirection;
matrix LightViewProj;

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
    
    outp.PositionPS = mul(inp.Position, WorldViewProjection);
    
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
    return mul(pos, WorldViewProjection);
}

float4 MainPS(VSOutput inp) : SV_Target0
{
    // sample the texture
    float2 texCoord;
    texCoord.x = (inp.TexCoord.x % 1) * TextureSize.x + inp.TextureBase.x;
    texCoord.y = (inp.TexCoord.y % 1) * TextureSize.y + inp.TextureBase.y;
    float4 diffuseColor = Texture.Sample(TextureSampler, texCoord);

    // sample the shadow map
    float4 lightingPosition = mul(inp.WorldPos, LightViewProj);
    float2 ShadowTexCoord = 0.5 * lightingPosition.xy / lightingPosition.w + float2(0.5, 0.5);
    ShadowTexCoord.y = 1.0f - ShadowTexCoord.y;

    float shadowdepth = DepthBuffer.Sample(DepthBufferSampler, ShadowTexCoord).r;
	
    // testing...
   
    float DepthBias = 0.0001;
    float ourdepth = (lightingPosition.z / lightingPosition.w) - DepthBias;

    
    float diffuseIntensity = saturate(dot(-LightDirection, inp.Normal));
	
    float4 AmbientColor = float4(0.15, 0.15, 0.15, 0);


    float4 diffuse = diffuseIntensity * diffuseColor + AmbientColor;
	
    if (shadowdepth < ourdepth)
    {
        diffuse *= float4(0.5, 0.5, 0.5, 0);
    };

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
