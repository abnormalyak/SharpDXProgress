using SharpDX;
using SharpDXPractice.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpDXPractice.Graphics
{
    public class DGraphics
    {
        private DDX11 D3D { get; set; }
        private DCamera Camera { get; set; }
        private DModel Model { get; set; }
        private DTextureShader TextureShader { get; set; }

        public DGraphics() { }

        public bool Initialize(DSystemConfiguration config, IntPtr windowHandle)
        {
            try
            {
                // Create Direct3D object
                D3D = new DDX11();

                // Initialize the Direct3D object
                if (!D3D.Initialize(config, windowHandle))
                    return false;
                
                // Create the camera object
                Camera = new DCamera();

                // Set initial position of camera
                Camera.SetPosition(0, 0, -10);
                
                // Create the model object
                Model = new DModel();

                // Initialize the model
                if (!Model.Initialize(D3D.Device, DSystemConfiguration.TextureFilePath + "sandstone.bmp"))
                {
                    MessageBox.Show("Could not initialize model object.");
                    return false;
                }
                
                // Create the color shader object
                TextureShader = new DTextureShader();

                // Initialize the color shader object
                if (!TextureShader.Initialize(D3D.Device, windowHandle))
                {
                    MessageBox.Show("Could not initialize the texture shader object.");
                    return false;
                }

                return true;
            } 
            catch (Exception ex)
            {
                MessageBox.Show("Could not initialize Direct3D:\n" + ex.Message);
                return false;
            }
        }
        public void ShutDown()
        {
            Camera = null;

            TextureShader?.ShutDown();
            TextureShader = null;

            Model?.ShutDown();
            Model = null;

            D3D?.ShutDown();
            D3D = null;
        }

        public bool Frame()
        {
            return Render();
        }

        public bool Render()
        {
            Matrix viewMatrix, projectionMatrix, worldMatrix;

            // Clear the buffers to begin the scene
            D3D.BeginScene(0.3f, 0, 0.1f, 1.0f);

            // Generate view matrix based on camera's position
            Camera.Render();

            // Get the world, view and projection matrices from the camera and D3D objects
            viewMatrix = Camera.ViewMatrix;
            worldMatrix = D3D.WorldMatrix;
            projectionMatrix = D3D.ProjectionMatrix;

            // Put the model vertex and index buffers on the graphics pipeline to prepare them from drawing
            Model.Render(D3D.DeviceContext);

            // Render the model using the colour shader
            if (!TextureShader.Render(D3D.DeviceContext, Model.IndexCount, worldMatrix, viewMatrix, projectionMatrix, Model.Texture.TextureResource))
            {
                MessageBox.Show("Texture shader failed");
                return false;
            }

            // Present the rendered scene to the screen
            D3D.EndScene();

            return true;
        }
    }
}
