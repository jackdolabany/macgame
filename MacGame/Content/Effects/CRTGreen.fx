#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler s0;

// Time parameter for ghosting animation
float Time;

float4 MainPS(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    // CRT curvature - subtle barrel distortion
    float2 uv = texCoord - 0.5; // Center coords
    float2 uvSquared = uv * uv;
    float distortion = 0.075; // Subtle curvature
    uv = uv + uv * distortion * (uvSquared.x + uvSquared.y);
    uv = uv + 0.5; // Move back to 0-1 range

    // Clip edges that go out of bounds (fade to black)
    float edge = step(0.0, uv.x) * step(uv.x, 1.0) * step(0.0, uv.y) * step(uv.y, 1.0);

    // Sample main texture
    float4 texColor = tex2D(s0, uv);

    // Map the 16-color palette to specific brightness levels for maximum distinction
    // Do this BEFORE ghosting so colors don't blend
    // Normalize to 0-1 range for comparison
    float3 col = texColor.rgb;

    // Define all 16 palette colors (normalized to 0-1 range)
    float3 pal_black = float3(0, 0, 0);
    float3 pal_darkBlue = float3(29, 43, 83) / 255.0;
    float3 pal_darkPurple = float3(126, 37, 83) / 255.0;
    float3 pal_darkGreen = float3(0, 135, 81) / 255.0;
    float3 pal_brown = float3(171, 82, 54) / 255.0;
    float3 pal_darkGray = float3(95, 87, 79) / 255.0;
    float3 pal_lightGray = float3(194, 195, 199) / 255.0;
    float3 pal_white = float3(255, 241, 232) / 255.0;
    float3 pal_red = float3(255, 0, 77) / 255.0;
    float3 pal_orange = float3(255, 163, 0) / 255.0;
    float3 pal_yellow = float3(255, 236, 39) / 255.0;
    float3 pal_lightGreen = float3(0, 228, 54) / 255.0;
    float3 pal_lightBlue = float3(41, 173, 255) / 255.0;
    float3 pal_lightPurple = float3(131, 118, 156) / 255.0;
    float3 pal_pink = float3(255, 119, 168) / 255.0;
    float3 pal_peach = float3(255, 204, 170) / 255.0;

    // Find closest palette color and assign specific luminance
    float minDist = 999.0;
    float luminance = 0.5;

    // Helper macro to check color distance and assign luminance
    #define CHECK_COLOR(palColor, lum) { \
        float dist = distance(col, palColor); \
        if (dist < minDist) { minDist = dist; luminance = lum; } \
    }

    CHECK_COLOR(pal_black, 0.0)
    CHECK_COLOR(pal_darkBlue, 0.16)
    CHECK_COLOR(pal_darkPurple, 0.22)
    CHECK_COLOR(pal_darkGreen, 0.28)
    CHECK_COLOR(pal_brown, 0.34)
    CHECK_COLOR(pal_darkGray, 0.40)
    CHECK_COLOR(pal_red, 0.46)
    CHECK_COLOR(pal_lightPurple, 0.52)
    CHECK_COLOR(pal_pink, 0.58)
    CHECK_COLOR(pal_lightBlue, 0.64)
    CHECK_COLOR(pal_lightGreen, 0.70)
    CHECK_COLOR(pal_orange, 0.74)
    CHECK_COLOR(pal_peach, 0.78)
    CHECK_COLOR(pal_yellow, 0.82)
    CHECK_COLOR(pal_lightGray, 0.92)
    CHECK_COLOR(pal_white, 1.0)

    #undef CHECK_COLOR

    // Map to green phosphor shades with higher contrast
    // Old CRT phosphors had a characteristic green color
    // Bright areas are bright green, dark areas are very dark
    float3 phosphorBlack = float3(0.0, 0.0, 0.0);      // Pure black
    float3 phosphorGreen = float3(0.2, 0.95, 0.25);    // Bright phosphor green

    texColor.rgb = lerp(phosphorBlack, phosphorGreen, luminance);

    // Ghosting effect - now applied to the green monochrome image
    // This preserves color distinctions while adding phosphor trails
    float pixelHeight = 1.0 / 224.0;
    float ghostOffset1 = pixelHeight * 1.5;
    float ghostOffset2 = pixelHeight * 3.0;

    // Sample and convert ghost pixels to monochrome
    float3 ghostCol1 = tex2D(s0, uv + float2(0.0, ghostOffset1)).rgb;
    float minDist1 = 999.0;
    float ghostLum1 = 0.5;
    #define CHECK_COLOR(palColor, lum) { \
        float dist = distance(ghostCol1, palColor); \
        if (dist < minDist1) { minDist1 = dist; ghostLum1 = lum; } \
    }
    CHECK_COLOR(pal_black, 0.0)
    CHECK_COLOR(pal_darkBlue, 0.16)
    CHECK_COLOR(pal_darkPurple, 0.22)
    CHECK_COLOR(pal_darkGreen, 0.28)
    CHECK_COLOR(pal_brown, 0.34)
    CHECK_COLOR(pal_darkGray, 0.40)
    CHECK_COLOR(pal_red, 0.46)
    CHECK_COLOR(pal_lightPurple, 0.52)
    CHECK_COLOR(pal_pink, 0.58)
    CHECK_COLOR(pal_lightBlue, 0.64)
    CHECK_COLOR(pal_lightGreen, 0.70)
    CHECK_COLOR(pal_orange, 0.74)
    CHECK_COLOR(pal_peach, 0.78)
    CHECK_COLOR(pal_yellow, 0.82)
    CHECK_COLOR(pal_lightGray, 0.92)
    CHECK_COLOR(pal_white, 1.0)
    #undef CHECK_COLOR
    float3 ghost1 = lerp(phosphorBlack, phosphorGreen, ghostLum1);

    float3 ghostCol2 = tex2D(s0, uv + float2(0.0, ghostOffset2)).rgb;
    float minDist2 = 999.0;
    float ghostLum2 = 0.5;
    #define CHECK_COLOR(palColor, lum) { \
        float dist = distance(ghostCol2, palColor); \
        if (dist < minDist2) { minDist2 = dist; ghostLum2 = lum; } \
    }
    CHECK_COLOR(pal_black, 0.0)
    CHECK_COLOR(pal_darkBlue, 0.16)
    CHECK_COLOR(pal_darkPurple, 0.22)
    CHECK_COLOR(pal_darkGreen, 0.28)
    CHECK_COLOR(pal_brown, 0.34)
    CHECK_COLOR(pal_darkGray, 0.40)
    CHECK_COLOR(pal_red, 0.46)
    CHECK_COLOR(pal_lightPurple, 0.52)
    CHECK_COLOR(pal_pink, 0.58)
    CHECK_COLOR(pal_lightBlue, 0.64)
    CHECK_COLOR(pal_lightGreen, 0.70)
    CHECK_COLOR(pal_orange, 0.74)
    CHECK_COLOR(pal_peach, 0.78)
    CHECK_COLOR(pal_yellow, 0.82)
    CHECK_COLOR(pal_lightGray, 0.92)
    CHECK_COLOR(pal_white, 1.0)
    #undef CHECK_COLOR
    float3 ghost2 = lerp(phosphorBlack, phosphorGreen, ghostLum2);

    // Blend ghosting trails - reduced to prevent washing out
    texColor.rgb = texColor.rgb + ghost1 * 0.15 + ghost2 * 0.05;

    // Apply edge mask
    texColor.rgb *= edge;

    // Scanlines using sin wave - balanced for contrast
    float scanline = sin(uv.y * 3.14159 * 224.0);
    scanline = scanline * 0.5 + 0.5; // Convert from -1,1 to 0,1
    texColor.rgb *= 0.80 + (scanline * 0.20);

    // Vignette - moderate for depth
    float2 vigUV = uv - 0.5;
    float dist = length(vigUV) * 2.0;
    float vignette = 1.0 - (dist * 0.35);
    texColor.rgb *= vignette;

    // Brightness boost
    texColor.rgb *= 1.05;

    // Add slight phosphor glow to bright areas
    float glow = pow(luminance, 3.0) * 0.2;
    texColor.rgb += float3(0.0, glow, glow * 0.3);

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
