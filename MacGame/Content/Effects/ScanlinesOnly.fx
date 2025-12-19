#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler s0;

// Make this much more obvious for testing
float ScanlineIntensity = 0.5;
float2 TextureSize = float2(640.0, 448.0);

float4 MainPS(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    // Sample texture
    float4 texColor = tex2D(s0, texCoord);

    // Simple scanline using sin wave - should be very visible
    float scanline = sin(texCoord.y * 3.14159 * 224.0); // 224 = half of 448
    scanline = scanline * 0.5 + 0.5; // Convert from -1,1 to 0,1

    // Darken based on scanline
    texColor.rgb *= 0.5 + (scanline * 0.5);

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
