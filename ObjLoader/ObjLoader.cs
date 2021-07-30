using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDXPractice.Graphics;

namespace SharpDXPractice.ObjLoader
{
    /// <summary>
    /// Read .obj files
    /// </summary>
    public static class ObjLoader
    {
        enum Types
        {
            Vertex,
            Normal,
            Texture
        }

        public static List<DModel.DModelFormat> ReadFile(string filename)
        {
            List<DModel.DModelFormat> ModelObject = new List<DModel.DModelFormat>();

            List<string> lines = null;
            
            try
            {
                lines = File.ReadLines(filename).ToList();

                foreach (string line in lines)
                {
                    if (line[0].Equals("v"))
                    {
                        var vertices = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        ModelObject.Add(new DModel.DModelFormat()
                        {
                            x = float.Parse(vertices[0]),
                            y = float.Parse(vertices[1]),
                            z = float.Parse(vertices[2]),
                            tu = 0.0f,
                            tv = 0.0f,
                            nx = 0.0f,
                            ny = 0.0f,
                            nz = 0.0f
                        });
                    }
                    else
                    {
                        return ModelObject;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ObjLoader: Could not find file. Error:\n" + ex.Message);
                return null;
            }
        }
    }
}
