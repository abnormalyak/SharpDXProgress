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
    public class ObjLoader
    {
        IEnumerable<string> fileLines = null;

        public ObjLoader(string fileName)
        {
            fileLines = File.ReadAllLines(fileName);
        }

        public void ImportObj(string destination)
        {
            FileStream fileSave = null;

            try
            {
                List<DMayaVertex> vertices =
                    (from line in fileLines
                     where line.Trim().StartsWith("v ")
                     select new DMayaVertex(line.Substring(1))).ToList();
                List<DMayaTexture> textures =
                    (from line in fileLines
                     where line.Trim().StartsWith("vt ")
                     select new DMayaTexture(line.Substring(2))).ToList();
                List<DMayaNormal> normals =
                    (from line in fileLines
                     where line.Trim().StartsWith("vn ")
                     select new DMayaNormal(line.Substring(2))).ToList();
                List<DMayaFace> faces =
                    (from line in fileLines
                     where line.Trim().StartsWith("f ")
                     select new DMayaFace(line.Substring(2))).ToList();

                StringBuilder saveFile = new StringBuilder();
                saveFile.AppendLine("Vertex Count: " + faces.Count * 3);
                saveFile.AppendLine();
                saveFile.AppendLine("Data: ");
                saveFile.AppendLine();

                foreach (var face in faces)
                {
                    foreach (var faceIndices in face.vertices)
                    {
                        var vertex = vertices[faceIndices.Vertex - 1];
                        var texture = textures[faceIndices.Texture - 1];
                        var normal = normals[faceIndices.Normal - 1];

                        saveFile.AppendFormat("{0} {1} {2} ", vertex.x, vertex.y, vertex.z);
                        saveFile.AppendFormat("{0} {1} ", texture.x, texture.y);
                        saveFile.AppendFormat("{0} {1} {2}", normal.x, normal.y, normal.z);
                        saveFile.AppendLine();
                    }
                }

                File.WriteAllText(destination, saveFile.ToString());
            }
            finally
            {
                if (fileSave != null)
                {
                    fileSave.Flush();
                    fileSave.Close();
                    fileSave.Dispose();
                    fileSave = null;
                }
            }
        }
    }
}
