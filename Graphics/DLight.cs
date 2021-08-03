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
        public Vector4 ambientColor { get; private set; }
        public Vector4 diffuseColor { get; private set; }
        public Vector3 direction { get; private set; }
        public Vector4 specularColor { get; private set; }
        public float specularPower { get; set; }

        public DLight() { }

        public void SetAmbientColor(float red, float green, float blue, float alpha)
        {
            ambientColor = new Vector4(red, green, blue, alpha);
        }

        public void SetDiffuseColor(float red, float green, float blue, float alpha)
        {
            diffuseColor = new Vector4(red, green, blue, alpha);
        }

        public void SetDirection(float x, float y, float z)
        {
            direction = new Vector3(x, y, z);
        }

        public void SetSpecularColor(float red, float green, float blue, float alpha)
        {
            specularColor = new Vector4(red, green, blue, alpha);
        }
    }
}
