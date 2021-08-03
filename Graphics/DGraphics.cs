using SharpDX;
using SharpDXPractice.Archive.Tut05;
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
        private DLightShader LightShader { get; set; }
        private DTextureShader TextureShader { get; set; }
        private DLight Light { get; set; }
        public static float rotation { get; set; }
        public DBitmap Bitmap { get; set; }

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
                Camera.SetPosition(0, 0, -5);
                
                // Create the model object
                Model = new DModel();

                // START Comment out when using bitmap
                /*
                // Initialize the model
                if (!Model.Initialize(D3D.Device, "sphere.txt", "watercolor.bmp"))
                {
                    MessageBox.Show("Could not initialize model object.");
                    return false;
                }
                
                // Create the color shader object
                LightShader = new DLightShader();

                // Initialize the color shader object
                if (!LightShader.Initialize(D3D.Device, windowHandle))
                {
                    MessageBox.Show("Could not initialize the light shader object.");
                    return false;
                }

                Light = new DLight();

                Light.SetAmbientColor(0.15f, 0.15f, 0.15f, 1.0f);
                Light.SetDiffuseColor(0.95f, 0.6f, 0, 1);
                Light.SetDirection(1, 0, 0);
                Light.specularPower = 32;
                Light.SetSpecularColor(1, 1, 1, 1);
                */
                // END

                // START If using bitmap, uncomment
                TextureShader = new DTextureShader();

                if (!TextureShader.Initialize(D3D.Device, windowHandle))
                {
                    MessageBox.Show("Could not initialize texture shader object.");
                    return false;
                }

                Bitmap = new DBitmap();

                if (!Bitmap.Initialize(D3D.Device, config.Width, config.Height, "watercolor.bmp", 256, 256))
                    return false;
                // END
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
            Light = null;

            LightShader?.ShutDown();
            LightShader = null;

            Camera = null;

            LightShader?.ShutDown();
            LightShader = null;

            Model?.ShutDown();
            Model = null;

            D3D?.ShutDown();
            D3D = null;
        }

        public bool Frame()
        {
            Rotate();

            return Render(rotation);
        }

        public bool Render(float rotation)
        {
            Matrix viewMatrix, projectionMatrix, worldMatrix, orthoMatrix;

            // Clear the buffers to begin the scene
            D3D.BeginScene(0.3f, 0, 0.1f, 1.0f);

            // Generate view matrix based on camera's position
            Camera.Render();

            // Get the world, view and projection matrices from the camera and D3D objects
            viewMatrix = Camera.ViewMatrix;
            worldMatrix = D3D.WorldMatrix;
            projectionMatrix = D3D.ProjectionMatrix;

            // Get ortho matrix
            orthoMatrix = D3D.OrthoMatrix;

            // START 3D rendering (comment out for 2D)
            /*
            // Rotate the world matrix by the rotation value (makes model spin)
            Matrix.RotationY(rotation, out worldMatrix);

            // Put the model vertex and index buffers on the graphics pipeline to prepare them from drawing
            Model.Render(D3D.DeviceContext);

            // Render the model using the colour shader
            if (!LightShader.Render(D3D.DeviceContext,
                Model.IndexCount,
                worldMatrix, viewMatrix, projectionMatrix,
                Model.Texture.TextureResource,
                Light.direction, Light.diffuseColor, Light.ambientColor,
                Light.specularPower, Light.specularColor,
                Camera.GetPosition()))
            {
                MessageBox.Show("Texture shader failed");
                return false;
            }
            */

            // START 2D rendering (comment out for 3D)
            D3D.TurnZBufferOff();

            if (!Bitmap.Render(D3D.DeviceContext, 100, 100))
            {
                return false;
            }

            if (!TextureShader.Render(D3D.DeviceContext, Bitmap.IndexCount, worldMatrix, viewMatrix, orthoMatrix, Bitmap.Texture.TextureResource))
                return false;
            // END

            // Present the rendered scene to the screen
            D3D.EndScene();

            return true;
        }

        public static void Rotate()
        {
            rotation += (float)Math.PI * 0.001f;

            if (rotation > 360)
                rotation -= 360;
        }
    }
}
