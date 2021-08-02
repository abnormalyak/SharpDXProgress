using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXPractice.ObjLoader
{
    public class DNormal
    {
        public float x;
        public float y;
        public float z;

        public DNormal(string normal)
        {
            var normalCoords = normal.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            x = float.Parse(normalCoords[0]);
            y = float.Parse(normalCoords[1]);
            z = float.Parse(normalCoords[2]);
        }
    }

    public class DMayaNormal : DNormal
    {
        public DMayaNormal(string normal)
            : base(normal)
        {
            z = -z;
        }
    }
}
