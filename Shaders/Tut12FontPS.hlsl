// Globals
Texture2D shaderTexture;
SamplerState SampleType;

cbuffer PixelBuffer
{
    float4 pixelColor;
};

// Typedefs
struct PixelInputType
{
    float4 position : SV_POSITION;
    float2 tex : TEXCOORD0;
};

float4 FontPixelShader(PixelInputType input) : SV_Target
{
    float4 color;
    
    color = shaderTexture.Sample(SampleType, input.tex);
    
    if (color.r == 0 && color.g == 0 && color.b == 0)
        color.a = 0;
    else
    {
        color.a = 1;
        color = color * pixelColor;
    }
    
    return color;
}