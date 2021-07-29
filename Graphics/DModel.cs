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
        public DTexture Texture { get; private set; }
        public DModel() { }

        public bool Initialize(D3D11.Device device, string texFileName)
        {
            if (!InitializeBuffers(device))
                return false;

            if (!LoadTexture(device, texFileName))
                return false;

            return true;
        }

        public void ShutDown()
        {
            ReleaseTexture();

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
                // Number of vertices in the vertex array
                VertexCount = 4;
                // Number of vertices in the index array
                IndexCount = 6;

                /**
                 * Note: The vertices are created in the clockwise order of drawing them.
                 * If done counter-clockwise, it'll think the triangle is facing the opposite direction
                 * and not draw it due to back-face culling.
                 * The order in which vertices are sent to the GPU Is very important.
                 */
                // Create vertex array, load it with data
                var vertices = new[]
                {
                    /*
                     * Exercise 4.2: Make the triangle red
                     * Have all color params as = new Vector4(1, 0, 0, 1)
                     */
                    // Bottom left
                    new DLightShader.DVertex()
                    {
                        position = new Vector3(-1, -1, 0),
                        texture = new Vector2(0, 1),
                        /*
                         * The normal is a line perpendicular to the face of the polygon
                         * so the exact direction the face is pointing can be calculated.
                         * Here, for simplicity, the normal for each vertex is along the 
                         * Z axis; setting each component to -1.0f makes the normal point
                         * towards the viewer (due to our current camera set-up of (0, 0, -10)).
                         */
                        normal = new Vector3(0, 0, -1.0f),
                    },
                    // Top left
                    new DLightShader.DVertex()
                    {
                        position = new Vector3(-1, 1, 0),
                        texture = new Vector2(0, 0),
                        normal = new Vector3(0, 0, -1.0f),
                    },
                    // Top right
                    new DLightShader.DVertex()
                    {
                        position = new Vector3(1, 1, 0),
                        texture = new Vector2(1, 0),
                        normal = new Vector3(0, 0, -1.0f),
                    },
                    // Bottom right
                    new DLightShader.DVertex()
                    {
                        position = new Vector3(1, -1, 0),
                        texture = new Vector2(1, 1),
                        normal = new Vector3(0, 0, -1.0f)
                    }
                };

                // Create indices for the index buffer
                int[] indices = new int[]
                {
                    0,  
                    1,  
                    2,  
                    0,
                    2,
                    3
                };

                // Create the vertex buffer
                VertexBuffer = D3D11.Buffer.Create(device, D3D11.BindFlags.VertexBuffer, vertices);

                // Create the index buffer
                IndexBuffer = D3D11.Buffer.Create(device, D3D11.BindFlags.IndexBuffer, indices);

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
                    Utilities.SizeOf<DLightShader.DVertex>(),
                    0));

            deviceContext.InputAssembler.SetIndexBuffer(
                IndexBuffer,
                SharpDX.DXGI.Format.R32_UInt,
                0);

            deviceContext.InputAssembler.PrimitiveTopology =
                SharpDX.Direct3D.PrimitiveTopology.TriangleList;
        }

        private bool LoadTexture(D3D11.Device device, string texFileName)
        {
            Texture = new DTexture();

            return Texture.Initialize(device, texFileName);
        }

        private void ReleaseTexture()
        {
            Texture?.ShutDown();
            Texture = null;
        }

    }
}
