#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler s0;

float4 MainPS(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    // Just pass through the texture with no effects
    float4 texColor = tex2D(s0, texCoord);
    return texColor * color;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
