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

        private SentenceType[] sentences = new SentenceType[6];


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
            // Used for mouse X position display
            if (!InitializeSentence(out sentences[0], 16, device))
                return false;

            if (!UpdateSentence(ref sentences[0], "Hello", 100, 100, 1, 1, 1, deviceContext))
                return false;

            // Used for mouse Y position display
            if (!InitializeSentence(out sentences[1], 16, device))
                return false;

            if (!UpdateSentence(ref sentences[1], "Goodbye", 100, 200, 1, 0, 0, deviceContext))
                return false;

            // Used for user's pressed keys display
            if (!InitializeSentence(out sentences[2], 64, device))
                return false;

            // Used for FPS counter
            if (!InitializeSentence(out sentences[3], 32, device))
                return false;

            // Used for CPU usage display
            if (!InitializeSentence(out sentences[4], 32, device))
                return false;

            // Used for render count display
            if (!InitializeSentence(out sentences[5], 32, device))
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
            sentence = new SentenceType();
            sentence.VertexBuffer = null;
            sentence.IndexBuffer = null;
            sentence.maxLength = maxLength;
            sentence.vertexCount = 6 * maxLength; 
            sentence.indexCount = 6 * maxLength;

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

        private bool UpdateSentence(ref SentenceType sentence, string message, int posX, int posY, float red, float green, float blue, DeviceContext deviceContext)
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
            mappedResource.WriteRange<DFont.DVertex>(vertices.ToArray());

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

        public bool SetMousePosition(int mouseX, int mouseY, DeviceContext deviceContext)
        {
            string mouseString = "Mouse X: " + mouseX.ToString();
            if (!UpdateSentence(ref sentences[0], mouseString, 20, 10, 1, 0, 1, deviceContext))
                return false;

            mouseString = "Mouse Y: " + mouseY.ToString();
            if (!UpdateSentence(ref sentences[1], mouseString, 20, 40, 1, 1, 0, deviceContext))
                return false;

            return true;
        }

        public bool SetPressedKeys(string pressedKeys, DeviceContext deviceContext)
        {
            return UpdateSentence(ref sentences[2], "Pressed keys: " + pressedKeys, 20, 70, 1, 0, 1, deviceContext);
        }

        public bool SetFps(int fps, DeviceContext deviceContext)
        {
            // Default colour to white
            float red = 1, green = 1, blue = 1;

            // Cap fps to 9999
            if (fps > 9999)
            {
                fps = 9999;
            }

            // Convert the fps to string format
            string fpsString = "FPS: " + fps.ToString();

            // If FPS >= 60, set colour to green
            if (fps >= 60)
                red = blue = 0;

            // If FPS < 60, set colour to yellow
            if (fps < 60)
                blue = 0;

            // If FPS < 30, set colour to red
            if (fps < 30)
                green = 0;

            return UpdateSentence(ref sentences[3], fpsString, 20, 100, red, green, blue, deviceContext);
        }

        public bool SetCpuUsage(int cpuUsage, DeviceContext deviceContext)
        {
            string cpuUsageString = "CPU: " + cpuUsage.ToString();

            return UpdateSentence(ref sentences[4], cpuUsageString, 20, 130, 0, 1, 0, deviceContext);
        }

        public bool SetRenderCount(int renderCount, DeviceContext deviceContext)
        {
            string renderCountString = "Render count: " + renderCount.ToString();

            return UpdateSentence(ref sentences[5], renderCountString, 20, 160, 0, 1, 0, deviceContext);
        }
    }
}
