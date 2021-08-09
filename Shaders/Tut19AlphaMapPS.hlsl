// Globals
Texture2D shaderTextures[3];
SamplerState SampleType;


// Typedefs
struct PixelInputType
{
    float4 position : SV_POSITION;
    float2 tex : TEXCOORD0;
};

float4 AlphaMapPixelShader(PixelInputType input) : SV_Target
{
    float4 color1, color2, alphaValue, blendColor;
    
    // Sample the textures
    color1 = shaderTextures[0].Sample(SampleType, input.tex);
    color2 = shaderTextures[1].Sample(SampleType, input.tex);
    alphaValue = shaderTextures[2].Sample(SampleType, input.tex);
    
    // Blend the two pixels together based on the alpha value
    blendColor = (alphaValue * color1) + ((1.0 - alphaValue) * color2);
    
    blendColor = saturate(blendColor);
    
    return blendColor;
}