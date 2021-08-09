// Globals
Texture2D shaderTextures[2];
SamplerState SampleType;


// Typedefs
struct PixelInputType
{
    float4 position : SV_POSITION;
    float2 tex : TEXCOORD0;
};

float4 MultiTexturePixelShader(PixelInputType input) : SV_Target
{
    float4 color1, color2, blendColor;
    
    // Sample the two textures
    color1 = shaderTextures[0].Sample(SampleType, input.tex);
    color2 = shaderTextures[1].Sample(SampleType, input.tex);
    
    // Blend the two pixels together, and multiply by the gamma value
    blendColor = color1 * color2 * 2.0;
    
    blendColor = saturate(blendColor);
    
    return blendColor;
}