using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXPractice.ObjLoader
{
    public class DTexture
    {
        public float x;
        public float y;

        public DTexture(string texture)
        {
            var texCoords = texture.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            x = float.Parse(texCoords[0]);
            y = float.Parse(texCoords[1]);
        }
    }

    public class DMayaTexture : DTexture
    {
        public DMayaTexture(string texture)
            : base(texture)
        {
            y = 1 - y;
        }
    }
}
