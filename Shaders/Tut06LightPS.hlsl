// Globals
Texture2D shaderTexture;
SamplerState SampleType;

cbuffer LightBuffer
{
    float4 diffuseColor;
    float3 lightDirection;
    float padding;
};

// Types
struct PixelInputType
{
    float4 position : SV_POSITION;
    float2 tex : TEXCOORD0;
    float3 normal : NORMAL;
};

float4 LightPixelShader(PixelInputType input) : SV_Target
{
    float4 textureColor;
    float3 lightDir;
    float lightIntensity;
    float4 color;
    
    // Sample pixel colour from texture
    textureColor = shaderTexture.Sample(SampleType, input.tex);
    
    // Invert light direction for calculations
    lightDir = -lightDirection;
    
    // Calculate the amount of light on this pixel
    lightIntensity = saturate(dot(input.normal, lightDir));
    
    // Determine final amount of diffuse colour based on diffuse colour combined w/ light intensity
    color = saturate(diffuseColor * lightIntensity);
    
    // Multiply texture pixel and final diffuse colour to get the final pixel colour
    color = color * textureColor;
    
    return color;

}