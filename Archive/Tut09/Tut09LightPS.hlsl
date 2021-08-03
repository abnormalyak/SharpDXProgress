// Globals
Texture2D shaderTexture;
SamplerState SampleType;

cbuffer LightBuffer
{
    float4 ambientColor;
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
    
    // Set the default output color to the ambient light value for all pixels
    color = ambientColor;
    
    // Invert light direction for calculations
    lightDir = -lightDirection;
    
    // Calculate the amount of light on this pixel
    lightIntensity = saturate(dot(input.normal, lightDir));
    
    if (lightIntensity > 0)
    {
        color += (diffuseColor * lightIntensity);

    }
    
    // Determine final amount of diffuse colour based on diffuse colour combined w/ light intensity
    color = saturate(color);
    
    // Multiply texture pixel and final diffuse colour to get the final pixel colour
    color = color * textureColor;
    
    return color;

}