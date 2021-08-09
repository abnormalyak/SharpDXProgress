// Globals
Texture2D shaderTextures[2];
SamplerState SampleType;


// Typedefs
struct PixelInputType
{
    float4 position : SV_POSITION;
    float2 tex : TEXCOORD0;
};

float4 LightMapPixelShader(PixelInputType input) : SV_Target
{
    float4 baseColor, lightColor, finalColor;
    
    // Sample the two textures
    baseColor = shaderTextures[0].Sample(SampleType, input.tex);
    lightColor = shaderTextures[1].Sample(SampleType, input.tex);
    
    // Blend the two pixels together, and multiply by the gamma value
    finalColor = baseColor * lightColor;
    
    return finalColor;
}