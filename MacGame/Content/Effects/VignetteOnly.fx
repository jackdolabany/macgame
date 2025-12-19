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
    // Sample texture
    float4 texColor = tex2D(s0, texCoord);

    // Calculate distance from center
    float2 vigUV = texCoord - 0.5;
    float dist = length(vigUV) * 2.0;

    // Apply vignette - hardcoded strength (0.5 for subtle effect)
    float vignette = 1.0 - (dist * 0.5);

    // Apply the vignette to the texture
    texColor.rgb = texColor.rgb * vignette;

    // Multiply by vertex color
    return texColor * color;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
