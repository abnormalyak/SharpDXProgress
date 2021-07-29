using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDXPractice.System;
using SharpDX.D3DCompiler;
using System.Windows.Forms;

namespace SharpDXPractice.Graphics
{
    public class DTextureShader
    {
        #region Structs
        [StructLayout(LayoutKind.Sequential)]
        internal struct DVertex
        {
            public Vector3 position;
            public Vector2 texture;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DMatrixBuffer
        {
            public Matrix world;
            public Matrix view;
            public Matrix projection;
        }
        #endregion

        #region Properties
        public VertexShader VertexShader { get; set; }
        public PixelShader PixelShader { get; set; }
        public InputLayout InputLayout { get; set; }
        public SharpDX.Direct3D11.Buffer ConstantMatrixBuffer { get; set; }
        public SamplerState SamplerState { get; set; }
        #endregion

        public DTextureShader() { }

        public bool Initialize(Device device, IntPtr windowHandle)
        {
            return InitializeShader(device, windowHandle, "Tut05TextureVS.hlsl", "Tut05TexturePS.hlsl");
        }

        /// <summary>
        /// Loads shader files and makes them usable to DX and the GPU
        /// </summary>
        /// <param name="device"></param>
        /// <param name="windowHandle"></param>
        /// <param name="vsFileName"></param>
        /// <param name="psFileName"></param>
        /// <returns></returns>
        private bool InitializeShader(Device device, IntPtr windowHandle, string vsFileName, string psFileName)
        {
            try
            {
                // Form full paths
                vsFileName = DSystemConfiguration.ShaderFilePath + vsFileName;
                psFileName = DSystemConfiguration.ShaderFilePath + psFileName;

                // Compile the vertex shader code
                ShaderBytecode vertexShaderByteCode = ShaderBytecode.CompileFromFile(
                    vsFileName,
                    "TextureVertexShader",
                    "vs_4_0",
                    ShaderFlags.None,
                    EffectFlags.None);
                ShaderBytecode pixelShaderByteCode = ShaderBytecode.CompileFromFile(
                    psFileName,
                    "TexturePixelShader",
                    "ps_4_0",
                    ShaderFlags.None,
                    EffectFlags.None);

                // Create the shaders from the buffer
                VertexShader = new VertexShader(device, vertexShaderByteCode);
                PixelShader = new PixelShader(device, pixelShaderByteCode);

                // Set up the layout of the data which goes into the shader
                InputElement[] inputElements = new InputElement[]
                {
                new InputElement()
                {
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    Format = SharpDX.DXGI.Format.R32G32B32_Float,
                    Slot = 0,
                    AlignedByteOffset = 0,
                    Classification = InputClassification.PerVertexData,
                    InstanceDataStepRate = 0
                },
                new InputElement()
                {
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    Format = SharpDX.DXGI.Format.R32G32_Float,
                    Slot = 0,
                    AlignedByteOffset = InputElement.AppendAligned,
                    Classification = InputClassification.PerVertexData,
                    InstanceDataStepRate = 0
                }
                };

                InputLayout = new InputLayout(device,
                    ShaderSignature.GetInputSignature(vertexShaderByteCode),
                    inputElements);

                // Release the vertex and pixel shader buffers
                vertexShaderByteCode.Dispose();
                pixelShaderByteCode.Dispose();

                // Set up the description of the dynamic matrix constant buffer in the vertex shader
                BufferDescription matrixBufferDesc = new BufferDescription()
                {
                    Usage = ResourceUsage.Dynamic,
                    SizeInBytes = Utilities.SizeOf<DMatrixBuffer>(),
                    BindFlags = BindFlags.ConstantBuffer,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    OptionFlags = ResourceOptionFlags.None,
                    StructureByteStride = 0
                };

                ConstantMatrixBuffer = new SharpDX.Direct3D11.Buffer(device, matrixBufferDesc);

                // Create a texture sampler state description
                SamplerStateDescription samplerDesc = new SamplerStateDescription()
                {
                    Filter = Filter.MinMagLinearMipPoint,
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Wrap,
                    AddressW = TextureAddressMode.Wrap,
                    MipLodBias = 0,
                    MaximumAnisotropy = 1,
                    ComparisonFunction = Comparison.Always,
                    BorderColor = new Color4(0, 0, 0, 0),
                    MinimumLod = 0,
                    MaximumLod = float.MaxValue
                };

                SamplerState = new SamplerState(device, samplerDesc);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing shader.\n" + ex.Message);
                return false;
            }
        }

        public void ShutDown()
        {
            ShutDownShader();
        }

        public bool Render(
            DeviceContext deviceContext, 
            int indexCount, 
            Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, 
            ShaderResourceView texture)
        {
            if (!SetShaderParameters(deviceContext, worldMatrix, viewMatrix, projectionMatrix, texture))
                return false;

            RenderShader(deviceContext, indexCount);

            return true;
        }


        /// <summary>
        /// Release resources that were set up in InitializeShader function
        /// </summary>
        public void ShutDownShader()
        {
            SamplerState?.Dispose();
            SamplerState = null;

            ConstantMatrixBuffer?.Dispose();
            ConstantMatrixBuffer = null;

            InputLayout?.Dispose();
            InputLayout = null;

            PixelShader?.Dispose();
            PixelShader = null;

            VertexShader?.Dispose();
            VertexShader = null;
        }
        private bool SetShaderParameters(
            DeviceContext deviceContext, 
            Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, 
            ShaderResourceView texture)
        {
            try
            {
                // Transpose matrices to prepare them for shader
                worldMatrix.Transpose();
                viewMatrix.Transpose();
                projectionMatrix.Transpose();

                // Lock the constant buffer so it can be written to
                DataStream mappedResource;
                deviceContext.MapSubresource(ConstantMatrixBuffer,
                    MapMode.WriteDiscard,
                    MapFlags.None,
                    out mappedResource);

                // Copy the matrices into the constant buffer
                DMatrixBuffer matrixBuffer = new DMatrixBuffer()
                {
                    world = worldMatrix,
                    view = viewMatrix,
                    projection = projectionMatrix
                };
                mappedResource.Write(matrixBuffer);

                // Unlock the constant buffer
                deviceContext.UnmapSubresource(ConstantMatrixBuffer, 0);

                // Set the position of the constant buffer in the vertex shader
                int bufferSlotNumber = 0;

                // Set the constant buffer in the vertex shader with the updated values
                deviceContext.VertexShader.SetConstantBuffer(bufferSlotNumber, ConstantMatrixBuffer);

                // Set shader resource in the pixel shader
                deviceContext.PixelShader.SetShaderResource(0, texture);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void RenderShader(DeviceContext deviceContext, int indexCount)
        {
            deviceContext.InputAssembler.InputLayout = InputLayout;

            deviceContext.VertexShader.Set(VertexShader);
            deviceContext.PixelShader.Set(PixelShader);

            deviceContext.PixelShader.SetSampler(0, SamplerState);

            deviceContext.DrawIndexed(indexCount, 0, 0);
        }
    }
}
