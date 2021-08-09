using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXPractice.Graphics
{
    public class DTextureArray
    {
        private Device _device;
        public List<DTexture> Textures { get; private set; }

        public bool Initialize(Device device, string[] filenames)
        {
            try
            {
                Textures = new List<DTexture>();
                _device = device;
                
                foreach(var filename in filenames)
                {
                    if (!AddFromFile(filename))
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Shutdown()
        {
            foreach (DTexture texture in Textures)
                texture.ShutDown();
        }

        private bool AddFromFile(string filename)
        {
            DTexture texture = new DTexture();
            if (!texture.Initialize(_device, filename))
                return false;

            Textures.Add(texture);

            return true;
        }

    }
}
