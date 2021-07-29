using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXPractice.Graphics
{
    public class DLight
    {
        public Vector4 diffuseColor { get; private set; }
        public Vector3 direction { get; private set; }

        public DLight() { }

        public void SetDiffuseColor(float red, float green, float blue, float alpha)
        {
            diffuseColor = new Vector4(red, green, blue, alpha);
        }

        public void SetDirection(float x, float y, float z)
        {
            direction = new Vector3(x, y, z);
        }
    }
}
