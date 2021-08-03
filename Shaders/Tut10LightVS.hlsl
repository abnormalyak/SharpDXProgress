// Globals
cbuffer MatrixBuffer
{
    matrix worldMatrix;
    matrix viewMatrix;
    matrix projectionMatrix;
};

cbuffer CameraBuffer
{
    float3 cameraPosition;
    float padding;
};

// Typedefs
struct VertexInputType
{
    float4 position : POSITION;
    float2 tex : TEXCOORD0;
    float3 normal : NORMAL;
};

struct PixelInputType
{
    float4 position : SV_POSITION;
    float2 tex : TEXCOORD0;
    float3 normal : NORMAL;
    float3 viewDirection : TEXCOORD1;
};

// Vertex shader
PixelInputType LightVertexShader(VertexInputType input)
{
    PixelInputType output;
    float4 worldPosition;
    
    input.position.w = 1.0f;
    
    output.position = mul(input.position, worldMatrix);
    output.position = mul(output.position, viewMatrix);
    output.position = mul(output.position, projectionMatrix);
    
    output.tex = input.tex;
    
    // Calculate the normal vector against world matrix
    output.normal = mul(input.normal, (float3x3)worldMatrix);
    
    // Normalize the output vector
    output.normal = normalize(output.normal);
    
    // Calculate the position of the vertex in 3D space
    worldPosition = mul(input.position, worldMatrix);
    
    // Determine viewing direction
    output.viewDirection = cameraPosition.xyz - worldPosition.xyz;
    
    output.viewDirection = normalize(output.viewDirection);
    
    return output;
}

