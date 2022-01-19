#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;

struct VertexShaderOutput
{
	float4 Position : SV_Position;
	float4 Color : COLOR0;
};

VertexShaderOutput MainVS(float4 pos : POSITION0, float4 col : COLOR0)
{
    VertexShaderOutput output;

	output.Position = mul(pos, WorldViewProjection);
	output.Color = col;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	return input.Color;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};