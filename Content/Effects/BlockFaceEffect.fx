#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

Texture2D<float4> Texture : register(t0);
sampler TextureSampler : register(s0);

Texture2DArray<float4> ShadowMap : register(t1);
sampler ShadowMapSampler : register(s1) =
sampler_state
{
    Texture = <ShadowMap>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

matrix World;
matrix ViewProjection;
float2 TextureSize;
float4 LineColor;
float3 LightDirection;
matrix LightViewProj[4];
float2 ShadowMapSize;

float ttt[] = { 10, 25, 60, 128 };

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
    float Depth : TEXCOORD3;
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
    outp.Normal = normalize((float3) mul(inp.Normal, (float3x4) World));
    outp.WorldPos = mul(inp.Position, World);
    outp.Depth = outp.PositionPS.z;// / outp.PositionPS.w;

    return outp;
}

VSShadowOutput MainShadowVS(float4 position : SV_Position)
{
    VSShadowOutput outp;
    
    outp.Position = mul(position, mul(World, ViewProjection));
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

    float q = inp.Depth;
    int sIndex = 0;

    if(q < 9.5)
    {
        sIndex = 0;
    }
    else if (q < 24.5)
    {
        sIndex = 1;
    }
    else if (q < 59.5)
    {
        sIndex = 2;
    }
    else if (q < 128)
    {
        sIndex = 3;
    }
    else
    {
        sIndex = 4;
    }

    //float z = saturate(inp.Depth / 32);
    //return float4(z, 0.5, 0.5, 1);
    
    float shadow = 0;
    
    float4 AmbientColor = float4(0.15, 0.15, 0.15, 0);
    float shadowStrength = 0.5;

    if (sIndex != 4)
    {
        matrix lvp = LightViewProj[sIndex];

        float4 lightingPosition = mul(inp.WorldPos, lvp);
        float2 shadowTextCoord = 0.5 * lightingPosition.xy / lightingPosition.w + float2(0.5, 0.5);
        shadowTextCoord.y = 1.0f - shadowTextCoord.y;

        float2 TexelSize = float2(1, 1) / ShadowMapSize;

        float DepthBias = 0.0003 + 0.001 * sIndex;

        float vertexDepth = (lightingPosition.z / lightingPosition.w) - DepthBias;
	
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                float2 samplePoint = shadowTextCoord + float2(x, y) * TexelSize;

                float shadowDepth = ShadowMap.Sample(ShadowMapSampler, float3(samplePoint, sIndex)).r;
                if (shadowDepth < vertexDepth)
                {
                    shadow += shadowStrength;
                }
            }
        }
    
        shadow /= 9;
    }
    
    float diffuseIntensity = max(0.2, saturate(dot(-LightDirection, inp.Normal)));
    float4 diffuse = diffuseIntensity * diffuseColor + AmbientColor;
	
    diffuse *= float4(1 - shadow, 1 - shadow, 1 - shadow, 0);
  
    // if (sIndex == 1 || sIndex == 3) { diffuse.r = 1; }

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
