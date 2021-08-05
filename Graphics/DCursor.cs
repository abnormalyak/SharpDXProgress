using SharpDX;
using SharpDX.Direct3D11;
using SharpDXPractice.Archive.Tut05;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DX11 = SharpDX.Direct3D11;

namespace SharpDXPractice.Graphics
{
    public class DCursor
    {
        #region config
        private int ScreenWidth { get; set; }
        private int ScreenHeight { get; set; }
        private const int NumTriangles = 2; // Cursor size
        #endregion
        #region gpu
        private Matrix BaseViewMatrix { get; set; }

        public DX11.Buffer VertexBuffer;
        public DX11.Buffer IndexBuffer;

        public int vertexCount, indexCount;
        #endregion
        #region position / display-related
        public int PosX { get; set; }
        public int PosY { get; set; }
        public float red, green, blue;

        public DTriangleColorShader Shader { get; set; }
        #endregion

    public DCursor() { }

        public bool Initialize(DX11.Device device, DX11.DeviceContext deviceContext, IntPtr windowHandle, int screenWidth, int screenHeight, Matrix baseViewMatrix)
        {
            // Store values
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
            BaseViewMatrix = baseViewMatrix;

            // Set values
            vertexCount = indexCount = NumTriangles * 3;

            // Create and initialize colour shader
            Shader = new DTriangleColorShader();
            if (!Shader.Initialize(device, windowHandle))
                return false;

            if (!InitializeCursor(device))
                return false;

            return true;
        }

        public bool InitializeCursor(DX11.Device device)
        {
            var vertices = new DTriangleColorShader.DVertex[vertexCount];
            var indices = new int[indexCount];

            for (var i = 0; i < indexCount; i++)
                indices[i] = i;

            var vertexBufferDesc = new BufferDescription()
            {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = Utilities.SizeOf<DFont.DVertex>() * vertexCount,
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            };

            VertexBuffer = DX11.Buffer.Create(device, vertices, vertexBufferDesc);
            IndexBuffer = DX11.Buffer.Create(device, BindFlags.IndexBuffer, indices);

            vertices = null;
            indices = null;

            return true;
        }

        public bool SetMousePositionAndColor(int mouseX, int mouseY, float red, float green, float blue, DeviceContext deviceContext)
        {
            PosX = mouseX;
            PosY = mouseY;

            this.red = red;
            this.green = green;
            this.blue = blue;

            return UpdateCursor(deviceContext);
        }

        public bool Render(DX11.DeviceContext deviceContext, Matrix worldMatrix, Matrix orthoMatrix)
        {
            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<DTriangleColorShader.DVertex>(), 0));
            deviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);

            deviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

            return Shader.Render(deviceContext, indexCount,
                worldMatrix, BaseViewMatrix, orthoMatrix);
        }

        private bool UpdateCursor(DeviceContext deviceContext)
        {
            float drawX = -(ScreenWidth >> 1) + PosX;
            float drawY = (ScreenHeight >> 1) - PosY;

            List<DTriangleColorShader.DVertex> vertices;
            BuildVertexArray(out vertices, drawX, drawY);

            #region Copy vertex array to vertex buffer
            DataStream mappedResource;

            // Lock vertex buffer so it can be written to
            deviceContext.MapSubresource(VertexBuffer, MapMode.WriteDiscard, MapFlags.None, out mappedResource);
            mappedResource.WriteRange<DTriangleColorShader.DVertex>(vertices.ToArray());

            deviceContext.UnmapSubresource(VertexBuffer, 0);
            #endregion

            vertices?.Clear();
            vertices = null;

            return true;
        }

        private void BuildVertexArray(out List<DTriangleColorShader.DVertex> vertices, float drawX, float drawY)
        {
            vertices = new List<DTriangleColorShader.DVertex>();
            for (int i = 0; i < NumTriangles; i++)
            {
                // Bottom left
                vertices.Add(new DTriangleColorShader.DVertex()
                {
                    position = new Vector3(drawX, drawY - 10, 0),
                    color = new Vector4(red, green, blue, 1)
                });
                // Top middle
                vertices.Add(new DTriangleColorShader.DVertex()
                {
                    position = new Vector3(drawX + 10, drawY + 10, 0),
                    color = new Vector4(red, green, blue, 1)
                });
                // Bottom right
                vertices.Add(new DTriangleColorShader.DVertex()
                {
                    position = new Vector3(drawX + 20, drawY - 10, 0),
                    color = new Vector4(red, green, blue, 1)
                });
                drawX += 20;
                drawY += 20;
            }
        }
    }
}
