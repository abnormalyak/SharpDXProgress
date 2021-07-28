using SharpDXPractice.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXPractice.Graphics
{
    public class DGraphics
    {
        private DDX11 D3D { get; set; }
        public bool Initialize(DSystemConfiguration config, IntPtr windowHandle)
        {
            // Create Direct3D object
            D3D = new DDX11();

            // Initialize the Direct3D object
            return D3D.Initialize(config, windowHandle);
        }
        public void ShutDown()
        {
            D3D?.ShutDown();
            D3D = null;
        }

        public bool Frame()
        {
            return Render();
        }

        public bool Render()
        {
            D3D.BeginScene(0.5f, 0.5f, 0.5f, 1.0f);
            D3D.EndScene();
            return true;
        }
    }
}
