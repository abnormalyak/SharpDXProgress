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
    public class DLightShader
    {
        #region Structs
        [StructLayout(LayoutKind.Sequential)]
        internal struct DVertex
        {
            public Vector3 position;
            public Vector2 texture;
            public Vector3 normal;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DMatrixBuffer
        {
            public Matrix world;
            public Matrix view;
            public Matrix projection;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DLightBuffer
        {
            public Vector4 ambientColor;
            public Vector4 diffuseColor;
            public Vector3 lightDirection;
            public float specularPower;
            public Vector4 specularColor;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DCameraBuffer
        {
            public Vector3 cameraPosition;
            public float padding;
        }
        #endregion

        #region Properties
        public VertexShader VertexShader { get; set; }
        public PixelShader PixelShader { get; set; }
        public InputLayout InputLayout { get; set; }
        public SharpDX.Direct3D11.Buffer ConstantMatrixBuffer { get; set; }
        public SharpDX.Direct3D11.Buffer ConstantLightBuffer { get; set; }
        public SharpDX.Direct3D11.Buffer ConstantCameraBuffer { get; set; }
        public SamplerState SamplerState { get; set; }
        #endregion

        public DLightShader() { }

        public bool Initialize(Device device, IntPtr windowHandle)
        {
            return InitializeShader(device, windowHandle, "Tut10LightVS.hlsl", "Tut10LightPS.hlsl");
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
                    "LightVertexShader",
                    "vs_4_0",
                    ShaderFlags.None,
                    EffectFlags.None);
                ShaderBytecode pixelShaderByteCode = ShaderBytecode.CompileFromFile(
                    psFileName,
                    "LightPixelShader",
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
                    },
                    new InputElement()
                    {
                        SemanticName = "NORMAL",
                        SemanticIndex = 0,
                        Format = SharpDX.DXGI.Format.R32G32B32_Float,
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
                    Filter = Filter.MinimumMinMagMipLinear,
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

                // Create light buffer
                BufferDescription lightBufferDesc = new BufferDescription()
                {
                    Usage = ResourceUsage.Dynamic,
                    SizeInBytes = Utilities.SizeOf<DLightBuffer>(),
                    BindFlags = BindFlags.ConstantBuffer,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    OptionFlags = ResourceOptionFlags.None,
                    StructureByteStride = 0
                };

                // Create the constant buffer pointer to allow access to the vertex shader constant buffer
                ConstantLightBuffer = new SharpDX.Direct3D11.Buffer(device, lightBufferDesc);

                var cameraBufferDesc = new BufferDescription()
                {
                    Usage = ResourceUsage.Dynamic,
                    SizeInBytes = Utilities.SizeOf<DCameraBuffer>(),
                    BindFlags = BindFlags.ConstantBuffer,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    OptionFlags = ResourceOptionFlags.None,
                    StructureByteStride = 0
                };

                ConstantCameraBuffer = new SharpDX.Direct3D11.Buffer(device, cameraBufferDesc);

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
            ShaderResourceView texture,
            Vector3 lightDirection,
            Vector4 diffuseColor,
            Vector4 ambientColor,
            float specularPower, Vector4 specularColor,
            Vector3 cameraPosition)
        {
            if (!SetShaderParameters(deviceContext, 
                worldMatrix, viewMatrix, projectionMatrix, 
                texture, 
                lightDirection, diffuseColor, ambientColor, specularPower, specularColor,
                cameraPosition))
                return false;

            RenderShader(deviceContext, indexCount);

            return true;
        }


        /// <summary>
        /// Release resources that were set up in InitializeShader function
        /// </summary>
        public void ShutDownShader()
        {
            ConstantLightBuffer?.Dispose();
            ConstantLightBuffer = null;

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
            ShaderResourceView texture, 
            Vector3 lightDirection, 
            Vector4 diffuseColor, 
            Vector4 ambientColor, 
            float specularPower, Vector4 specularColor, 
            Vector3 cameraPosition)
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

                // Lock light constant buffer so it cna be written to
                deviceContext.MapSubresource(ConstantLightBuffer,
                    MapMode.WriteDiscard,
                    MapFlags.None,
                    out mappedResource);

                // Copy lighting variables into constant buffer
                DLightBuffer lightBuffer = new DLightBuffer()
                {
                    ambientColor = ambientColor,
                    diffuseColor = diffuseColor,
                    lightDirection = lightDirection,
                    specularPower = specularPower,
                    specularColor = specularColor
                };
                mappedResource.Write(lightBuffer);

                // Unlock constant buffer
                deviceContext.UnmapSubresource(ConstantLightBuffer, 0);

                bufferSlotNumber = 0;

                deviceContext.PixelShader.SetConstantBuffer(bufferSlotNumber, ConstantLightBuffer);

                // Lock camera constant buffer so it can be written to
                deviceContext.MapSubresource(ConstantCameraBuffer,
                    MapMode.WriteDiscard,
                    MapFlags.None,
                    out mappedResource);

                // Copy camera variables into constant buffer
                
                DCameraBuffer cameraBuffer = new DCameraBuffer()
                {
                    cameraPosition = cameraPosition,
                    padding = 0.0f
                };
                mappedResource.Write(cameraBuffer);

                deviceContext.UnmapSubresource(ConstantCameraBuffer, 0);

                bufferSlotNumber = 1;

                deviceContext.VertexShader.SetConstantBuffer(bufferSlotNumber, ConstantCameraBuffer);

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
