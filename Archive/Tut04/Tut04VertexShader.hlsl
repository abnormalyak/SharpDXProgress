// Globals
float4x4 worldMatrix;
float4x4 viewMatrix;
float4x4 projectionMatrix;

// Type definitions
struct VertexInputType
{
    float4 position : POSITION;
    float4 color : COLOR;
};

struct PixelInputType
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
};

// Vertex shader
PixelInputType ColorVertexShader(VertexInputType input)
{
    PixelInputType output;
    
    // Change the position vector to be 4 units for proper matrix calculations
    input.position.w = 1.0f;
    
    // Calculate the position of the vertex vs. the world, view and projection matrices
    output.position = mul(input.position, worldMatrix);
    output.position = mul(output.position, viewMatrix);
    output.position = mul(output.position, projectionMatrix);
    
    // Store the input colour for the pixel shader to use
    output.color = input.color;
    
    return output;
}