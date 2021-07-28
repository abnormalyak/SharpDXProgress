using D3D11 = SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace SharpDXPractice.Graphics
{
    public class DModel
    {
        private D3D11.Buffer VertexBuffer { get; set; }
        private D3D11.Buffer IndexBuffer { get; set; }
        private int VertexCount { get; set; }
        public int IndexCount { get; set; }
        public DModel() { }

        public bool Initialize(D3D11.Device device)
        {
            return InitializeBuffers(device);
        }

        public void ShutDown()
        {
            ShutDownBuffers();
        }

        public void Render(D3D11.DeviceContext deviceContext)
        {
            RenderBuffers(deviceContext);
        }

        private bool InitializeBuffers(D3D11.Device device)
        {
            try
            {
                VertexCount = 3;
                IndexCount = 3;

                /**
                 * Note: The vertices are created in the clockwise order of drawing them.
                 * If done counter-clockwise, it'll think the triangle is facing the opposite direction
                 * and not draw it due to back-face culling.
                 * The order in which vertices are sent to the GPU Is very important.
                 */
                // Create vertex array, load it with data
                var vertices = new[]
                {
                    // Bottom left
                    new DTriangleColorShader.DVertex()
                    {
                        position = new Vector3(-1, -1, 0),
                        color = new Vector4(1, 0, 0, 1)
                    },
                    // Top middle
                    new DTriangleColorShader.DVertex()
                    {
                        position = new Vector3(0, 1, 0),
                        color = new Vector4(0, 1, 0, 1)
                    },
                    // Bottom right
                    new DTriangleColorShader.DVertex()
                    {
                        position = new Vector3(1, -1, 0),
                        color = new Vector4(0, 0, 1, 1)
                    }
                };

                // Create indices for the index buffer
                int[] indices = new int[]
                {
                    0,  // Bottom left
                    1,  // Top middle
                    2   // Bottom right
                };

                // Create the vertex buffer
                VertexBuffer = D3D11.Buffer.Create(device, D3D11.BindFlags.VertexBuffer, vertices);

                // Create the index buffer
                IndexBuffer = D3D11.Buffer.Create(device, D3D11.BindFlags.IndexBuffer, indices);

                // Delete unneeded arrays
                vertices = null;
                indices = null;

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Release the vertex and index buffers
        /// </summary>
        private void ShutDownBuffers()
        {
            IndexBuffer?.Dispose();
            IndexBuffer = null;

            VertexBuffer?.Dispose();
            VertexBuffer = null;
        }

        /// <summary>
        /// Set the vertex buffer and index buffer as active on the input assembler
        /// in the GPU. Once the GPU has an active vertex buffer, it can use the HLSL
        /// shader to render that buffer.
        /// This method also defines how buffers should be drawn (the primitive topology).
        /// </summary>
        /// <param name="device"></param>
        private void RenderBuffers(D3D11.DeviceContext deviceContext)
        {
            deviceContext.InputAssembler.SetVertexBuffers(
                0,
                new D3D11.VertexBufferBinding(
                    VertexBuffer,
                    Utilities.SizeOf<DTriangleColorShader.DVertex>(),
                    0));

            deviceContext.InputAssembler.SetIndexBuffer(
                IndexBuffer,
                SharpDX.DXGI.Format.R32_UInt,
                0);

            deviceContext.InputAssembler.PrimitiveTopology =
                SharpDX.Direct3D.PrimitiveTopology.TriangleList;
        }

    }
}
