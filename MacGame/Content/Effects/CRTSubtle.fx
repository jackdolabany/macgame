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
    // CRT curvature - half intensity
    float2 uv = texCoord - 0.5; // Center coords
    float2 uvSquared = uv * uv;
    float distortion = 0.075; // Half of original (was 0.15)
    uv = uv + uv * distortion * (uvSquared.x + uvSquared.y);
    uv = uv + 0.5; // Move back to 0-1 range

    // Clip edges that go out of bounds (fade to black)
    float edge = step(0.0, uv.x) * step(uv.x, 1.0) * step(0.0, uv.y) * step(uv.y, 1.0);

    // Chromatic aberration - half intensity
    float2 centerOffset = uv - 0.5;
    float aberrationStrength = length(centerOffset) * 0.0075; // Half of original (was 0.015)

    float pixelWidth = 1.0 / 640.0;

    // Sample red channel shifted outward, blue inward, green centered
    float r = tex2D(s0, uv + centerOffset * aberrationStrength).r;
    float g = tex2D(s0, uv).g;
    float b = tex2D(s0, uv - centerOffset * aberrationStrength).b;

    // Pixel smearing on green channel for consistency
    g = g * 0.5;
    g += tex2D(s0, uv + float2(pixelWidth, 0.0)).g * 0.25;
    g += tex2D(s0, uv - float2(pixelWidth, 0.0)).g * 0.25;

    float4 texColor = float4(r, g, b, 1.0);

    // Apply edge mask
    texColor.rgb *= edge;

    // Scanlines - more subtle
    float scanline = sin(uv.y * 3.14159 * 224.0);
    scanline = scanline * 0.5 + 0.5; // Convert from -1,1 to 0,1
    texColor.rgb *= 0.875 + (scanline * 0.125); // Half intensity (was 0.75 + 0.25)

    // Vignette - half intensity
    float2 vigUV = uv - 0.5;
    float dist = length(vigUV) * 2.0;
    float vignette = 1.0 - (dist * 0.2); // Half of original (was 0.4)
    texColor.rgb *= vignette;

    // Brightness boost - more subtle
    texColor.rgb *= 1.075; // Half of boost (was 1.15)

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
