using SharpDX;
using SharpDX.Direct3D11;
using SharpDXPractice.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpDXPractice.Graphics
{
    public class DFont
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct FontType
        {
            public float left;
            public float right;
            public int size;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct DVertex
        {
            public Vector3 position;
            public Vector2 texture;
        };

        public List<FontType> FontCharacters { get; private set; }
        public DTexture Texture { get; private set; }

        public DFont() { }

        public bool Initialize(Device device, string fontFileName, string textureFileName)
        {
            if (!LoadFontData(fontFileName))
                return false;

            if (!LoadTexture(device, textureFileName))
                return false;

            return true;
        }

        public void ShutDown()
        {
            ReleaseTexture();

            ReleaseFontData();
        }

        private bool LoadFontData(string filename)
        {
            filename = DSystemConfiguration.ModelFilePath + filename;

            try
            {
                var fontData = new FontType[95];
                var lines = File.ReadAllLines(filename);
                FontCharacters = new List<FontType>();
                foreach (var line in lines)
                {
                    string[] splitLine = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    FontCharacters.Add(new FontType()
                    {
                        left = float.Parse(splitLine[splitLine.Length - 3]),
                        right = float.Parse(splitLine[splitLine.Length - 2]),
                        size = int.Parse(splitLine[splitLine.Length - 1])
                    });
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading font:\n" + ex.Message);
                return false;
            }
        }

        private void ReleaseFontData()
        {
            FontCharacters?.Clear();
            FontCharacters = null;
        }

        private bool LoadTexture(Device device, string filename)
        {
            filename = DSystemConfiguration.TextureFilePath + filename;
            Texture = new DTexture();

            return Texture.Initialize(device, filename);
        }

        private void ReleaseTexture()
        {
            Texture?.ShutDown();
            Texture = null;
        }

        /// <summary>
        /// Used by DText to build vertex buffers out of the text sentences it sends to this
        /// function as input. Thus, each sentence in DText that must be drawn has its own
        /// vertex buffer, which can be rendered easily after creation.
        /// </summary>
        /// <param name="vertices">The vertex array that will be returned</param>
        /// <param name="sentence">The text sequence used to create the vertex array</param>
        /// <param name="drawX">The X screen coordinate of where to draw the sentence</param>
        /// <param name="drawY">The Y screen coordinate of where to draw the sentence</param>
        public void BuildVertexArray(out List<DVertex> vertices, string sentence, float drawX, float drawY)
        {
            vertices = new List<DVertex>();

            // Build the vertex and index arrays, taking each character from the
            // sentence and creating two triangles for it.
            foreach (char c in sentence)
            {
                var letter = c - 32;

                if (letter == 0)
                    drawX += 3;
                else
                {
                    // First triangle in quad
                    vertices.Add(new DVertex()
                    {
                        // Top left
                        position = new Vector3(drawX, drawY, 0),
                        texture = new Vector2(FontCharacters[letter].left, 0)
                    });
                    vertices.Add(new DVertex()
                    {
                        // Bottom right
                        position = new Vector3(drawX + FontCharacters[letter].size, drawY - 16, 0),
                        texture = new Vector2(FontCharacters[letter].right, 1)
                    });
                    vertices.Add(new DVertex()
                    {
                        // Bottom left
                        position = new Vector3(drawX, drawY - 16, 0),
                        texture = new Vector2(FontCharacters[letter].left, 1)
                    });

                    // Second triangle in quad
                    vertices.Add(new DVertex()
                    {
                        // Top left
                        position = new Vector3(drawX, drawY, 0),
                        texture = new Vector2(FontCharacters[letter].left, 0)
                    });
                    vertices.Add(new DVertex()
                    {
                        // Top right
                        position = new Vector3(drawX + FontCharacters[letter].size, drawY, 0),
                        texture = new Vector2(FontCharacters[letter].right, 0)
                    });
                    vertices.Add(new DVertex()
                    {
                        // Bottom right
                        position = new Vector3(drawX + FontCharacters[letter].size, drawY - 16, 0),
                        texture = new Vector2(FontCharacters[letter].right, 1)
                    });

                    drawX += FontCharacters[letter].size + 1;
                }
            }
        }
    }
}
