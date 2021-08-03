// Globals
Texture2D shaderTexture;
SamplerState SampleType;

cbuffer LightBuffer
{
    float4 ambientColor;
    float4 diffuseColor;
    float3 lightDirection;
    float specularPower;
    float4 specularColor;
};

// Types
struct PixelInputType
{
    float4 position : SV_POSITION;
    float2 tex : TEXCOORD0;
    float3 normal : NORMAL;
    float3 viewDirection : TEXCOORD1;
};

float4 LightPixelShader(PixelInputType input) : SV_Target
{
    float4 textureColor;
    float3 lightDir;
    float lightIntensity;
    float4 color;
    float3 reflection;
    float4 specular;
    
    // Sample pixel colour from texture
    textureColor = shaderTexture.Sample(SampleType, input.tex);
    
    // Set the default output color to the ambient light value for all pixels
    color = ambientColor;
    
    // Initialize specular color
    specular = float4(0, 0, 0, 0);
    
    // Invert light direction for calculations
    lightDir = -lightDirection;
    
    // Calculate the amount of light on this pixel
    lightIntensity = saturate(dot(input.normal, lightDir));
    
    if (lightIntensity > 0)
    {
        //Determine final amount of diffuse colour based on diffuse colour combined w/ light intensity
        color += (diffuseColor * lightIntensity);

        // Sature the ambient and diffuse color
        color = saturate(color);
        
        // Calculate reflection vector
        reflection = normalize(2 * lightIntensity * input.normal - lightDir);
        
        // Calculate amount of specular light using reflection vector and viewing direction
        // The smaller the angle between the viewer and the light source, the greater the
        // specular light reflection will be.
        specular = pow(saturate(dot(reflection, input.viewDirection)), specularPower);
    }
    
    
    // Multiply texture pixel and final diffuse colour to get the final pixel colour
    color = color * textureColor;
    
    // Add the specular effect at the end; it is a highlight so must be added to final value
    color = saturate(color + specular);
    
    return color;

}