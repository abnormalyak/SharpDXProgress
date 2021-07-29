// Type definitions
struct PixelInputType
{
    float4 position : SV_Position;
    float4 color : COLOR;
};

// Pixel shader
float4 ColorPixelShader(PixelInputType input) : SV_Target
{
    return input.color;
}