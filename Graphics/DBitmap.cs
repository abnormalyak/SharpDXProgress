using SharpDX;
using SharpDX.Direct3D11;
using SharpDXPractice.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXPractice.Graphics
{
    public class DBitmap
    {
        public SharpDX.Direct3D11.Buffer VertexBuffer { get; set; }
        public SharpDX.Direct3D11.Buffer IndexBuffer { get; set; }
        public int VertexCount { get; set; }
        public int IndexCount { get; private set; }
        public DTexture Texture { get; private set; }
        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }
        public int BitmapWidth { get; private set; }
        public int BitmapHeight { get; private set; }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DVertex
        {
            public Vector3 position;
            public Vector2 texture;
        }

        public DBitmap() { }

        public bool Initialize(Device device, int screenWidth, int screenHeight, string textureFileName, int bitmapWidth, int bitmapHeight)
        {
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
            BitmapWidth = bitmapWidth;
            BitmapHeight = bitmapHeight;

            if (!InitializeBuffers(device))
                return false;

            if (!LoadTexture(device, textureFileName))
                return false;

            return true;
        }

        public void ShutDown()
        {
            ReleaseTexture();

            ShutDownBuffers();
        }

        private void ShutDownBuffers()
        {
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
        }

        public bool Render(DeviceContext deviceContext, int posX, int posY)
        {
            if (!UpdateBuffers(deviceContext, posX, posY))
                return false;

            RenderBuffers(deviceContext);

            return true;
        }

        private bool InitializeBuffers(Device device)
        {
            try
            {
                VertexCount = 4;
                IndexCount = 6;

                var vertices = new DVertex[VertexCount];
                var indices = new int[]
                {
                    0,
                    1,
                    2,
                    0,
                    3,
                    1
                };

                var vertexBuffer = new BufferDescription()
                {
                    Usage = ResourceUsage.Dynamic,
                    SizeInBytes = Utilities.SizeOf<DVertex>() * VertexCount,
                    BindFlags = BindFlags.VertexBuffer,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    OptionFlags = ResourceOptionFlags.None,
                    StructureByteStride = 0
                };

                VertexBuffer = SharpDX.Direct3D11.Buffer.Create(device, vertices, vertexBuffer);
                IndexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.IndexBuffer, indices);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool UpdateBuffers(DeviceContext deviceContext, int posX, int posY)
        {
            var left = (-(ScreenWidth >> 2)) + (float)posX;
            var right = left + BitmapWidth;
            var top = (ScreenHeight >> 2) - (float)posY;
            var bottom = top - BitmapHeight;

            var vertices = new[]
            {
                new DVertex()
                {
                    position = new Vector3(left, top, 0),
                    texture = new Vector2(0, 0)
                },
                new DVertex()
                {
                    position = new Vector3(right, bottom, 0),
                    texture = new Vector2(1, 1)
                },
                new DVertex()
                {
                    position = new Vector3(left, bottom, 0),
                    texture = new Vector2(0, 1)
                },
                new DVertex()
                {
                    position = new Vector3(right, top, 0),
                    texture = new Vector2(1, 0)
                }
            };

            DataStream mappedResource;

            deviceContext.MapSubresource(VertexBuffer, MapMode.WriteDiscard, MapFlags.None, out mappedResource);

            mappedResource.WriteRange<DVertex>(vertices);

            deviceContext.UnmapSubresource(VertexBuffer, 0);

            return true;
        }

        private void RenderBuffers(DeviceContext deviceContext)
        {
            var stride = Utilities.SizeOf<DVertex>();
            var offset = 0;

            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, stride, offset));

            deviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);

            deviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
        }

        private bool LoadTexture(Device device, string textureFileName)
        {
            textureFileName = DSystemConfiguration.TextureFilePath + textureFileName;

            Texture = new DTexture();

            Texture.Initialize(device, textureFileName);

            return true;
        }

        private void ReleaseTexture()
        {
            Texture?.ShutDown();
            Texture = null;
        }
    }
}
