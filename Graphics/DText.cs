using SharpDX;
using SharpDX.Direct3D11;
using SharpDXPractice.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DX11 = SharpDX.Direct3D11;

namespace SharpDXPractice.Graphics
{
    public class DText
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct SentenceType
        {
            public DX11.Buffer VertexBuffer;
            public DX11.Buffer IndexBuffer;
            public int vertexCount, indexCount, maxLength;
            public float red, green, blue;
        }

        private DFont Font { get; set; }
        private DFontShader FontShader { get; set; }
        private int ScreenWidth { get; set; }
        private int ScreenHeight { get; set; }
        private Matrix BaseViewMatrix { get; set; }

        private SentenceType[] sentences = new SentenceType[2];


        public DText() { }

        public bool Initialize(DX11.Device device, DX11.DeviceContext deviceContext, IntPtr windowHandle, int screenWidth, int screenHeight, Matrix baseViewMatrix)
        {
            // Set values
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
            BaseViewMatrix = baseViewMatrix;

            // Create and initialize the Font object
            Font = new DFont();

            if (!Font.Initialize(device, "fontdata.txt", "font.bmp"))
                return false;

            // Create and initalize the Font Shader object
            FontShader = new DFontShader();

            if (!FontShader.Initialize(device, windowHandle))
                return false;

            // Create and initialize strings to display
            if (!InitializeSentence(out sentences[0], 16, device))
                return false;

            if (!UpdateSentence(sentences[0], "Hello", 100, 100, 1, 1, 1, deviceContext))
                return false;

            if (!InitializeSentence(out sentences[1], 16, device))
                return false;

            if (!UpdateSentence(sentences[1], "Goodbye", 100, 200, 1, 0, 1, deviceContext))
                return false;

            return true;
        }

        public void ShutDown()
        {
            for(int i = 0; i < sentences.Length; i++)
            {
                sentences[i].VertexBuffer?.Dispose();
                sentences[i].VertexBuffer = null;

                sentences[i].IndexBuffer?.Dispose();
                sentences[i].IndexBuffer = null;
            }
        }

        public bool Render(DX11.DeviceContext deviceContext, Matrix worldMatrix, Matrix orthoMatrix)
        {
            foreach (SentenceType sentence in sentences)
            {
                if (!RenderSentence(deviceContext, sentence, worldMatrix, orthoMatrix))
                    return false;
            }

            return true;
        }


        private bool InitializeSentence(out SentenceType sentence, int maxLength, Device device)
        {
            sentence = new SentenceType()
            {
                VertexBuffer = null,
                IndexBuffer = null,
                maxLength = maxLength,
                vertexCount = 6 * maxLength,
                indexCount = 6 * maxLength
            };

            var vertices = new DFont.DVertex[sentence.vertexCount];
            var indices = new int[sentence.indexCount];

            for (var i = 0; i < sentence.indexCount; i++)
                indices[i] = i;

            var vertexBufferDesc = new BufferDescription()
            {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = Utilities.SizeOf<DFont.DVertex>() * sentence.vertexCount,
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            };

            sentence.VertexBuffer = DX11.Buffer.Create(device, vertices, vertexBufferDesc);
            sentence.IndexBuffer = DX11.Buffer.Create(device, BindFlags.IndexBuffer, indices);

            vertices = null;
            indices = null;

            return true;
        }

        private bool UpdateSentence(SentenceType sentence, string message, int posX, int posY, float red, float green, float blue, DeviceContext deviceContext)
        {
            // Set the color and size of the sentence
            sentence.red = red;
            sentence.green = green;
            sentence.blue = blue;

            // Get the number of letters in the sequence
            var numLetters = message.Length;

            // Check for potential buffer overflow
            if (numLetters > sentence.maxLength)
                return false;

            // Calculate X and Y pixel position on the screen to start drawing to
            float drawX = -(ScreenWidth >> 1) + posX;
            float drawY = (ScreenHeight >> 1) - posY;

            // Build vertex array
            List<DFont.DVertex> vertices;
            Font.BuildVertexArray(out vertices, message, drawX, drawY);

            #region Copy vertex array to sentence vertex buffer
            DataStream mappedResource;

            // Lock vertex buffer so it can be written to
            deviceContext.MapSubresource(sentence.VertexBuffer, MapMode.WriteDiscard, MapFlags.None, out mappedResource);
            mappedResource.WriteRange(vertices.ToArray());

            deviceContext.UnmapSubresource(sentence.VertexBuffer, 0);
            #endregion

            vertices?.Clear();
            vertices = null;

            return true;
        }

        /// <summary>
        /// Puts the sentence vertex and index buffers on the input assembler.
        /// Then calls the Font Shader object to draw the sentence given as 
        /// input to this function.
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        private bool RenderSentence(DeviceContext deviceContext, SentenceType sentence, Matrix worldMatrix, Matrix orthoMatrix)
        {
            // Set vertex buffer stride and offset
            int stride = Utilities.SizeOf<DFont.DVertex>();
            int offset = 0;

            // Set the vertex & index buffers to active in the IA so they can be rendered
            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(sentence.VertexBuffer, stride, offset));
            deviceContext.InputAssembler.SetIndexBuffer(sentence.IndexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);

            // Set the type of primitive to be rendered from this vertex buffer
            deviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

            Vector4 pixelColor = new Vector4(sentence.red, sentence.green, sentence.blue, 1);

            return FontShader.Render(deviceContext, sentence.indexCount, 
                worldMatrix, BaseViewMatrix, orthoMatrix, 
                Font.Texture.TextureResource, pixelColor);
        }
    }
}
