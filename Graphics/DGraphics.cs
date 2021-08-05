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
        public DText Text { get; set; }
        public DCursor Cursor { get; set; }
        #region Cursor properties
        private float red, green, blue; // Used by cursor
        private float pulseStage = 0;
        private int fadeTime = 5; // The time (in seconds) to fade between colours
        #endregion

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

                // START If rendering text, uncomment
                Camera.SetPosition(0, 0, -1);
                Camera.Render();
                var baseViewMatrix = Camera.ViewMatrix;

                Text = new DText();

                if (!Text.Initialize(D3D.Device, D3D.DeviceContext, windowHandle, config.Width, config.Height, baseViewMatrix))
                    return false;
                // END

                // Create the model object
                Model = new DModel();

                // START If rendering 3D models, uncomment

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

                // END

                // START For rendering cursor, uncomment
                Cursor = new DCursor();
                red = green = blue = 1;

                if (!Cursor.Initialize(D3D.Device, D3D.DeviceContext, windowHandle, config.Width, config.Height, baseViewMatrix))
                {
                    MessageBox.Show("Could not initialize cursor object.");
                    return false;
                }
                // END

                // START If using bitmap, uncomment
                /*
                TextureShader = new DTextureShader();

                if (!TextureShader.Initialize(D3D.Device, windowHandle))
                {
                    MessageBox.Show("Could not initialize texture shader object.");
                    return false;
                }

                Bitmap = new DBitmap();

                if (!Bitmap.Initialize(D3D.Device, config.Width, config.Height, "watercolor.bmp", 256, 256))
                    return false;
                */
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
            Text?.ShutDown();
            Text = null;

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

        public bool Frame(int mouseX, int mouseY, string pressedKeys, bool pulseCursorColor = false)
        {
            bool resultMouse = true, resultKeyboard = true;
            Rotate();

            // Set the location of the mouse
            if (!Text.SetMousePosition(mouseX, mouseY, D3D.DeviceContext))
                resultMouse = false;

            // Set the keys currently being pressed
            if (!Text.SetPressedKeys(pressedKeys, D3D.DeviceContext))
                resultKeyboard = false;

            // Set the location of the mouse for the cursor object
            if (!Cursor.SetMousePositionAndColor(mouseX, mouseY, red, green, blue, D3D.DeviceContext))
                return false;

            if (pulseCursorColor)
                PulseCursorColor();

            // Set the position of the camera
            Camera.SetPosition(0, 0, -5);

            //return Render(rotation);
            return (resultMouse | resultKeyboard);
        }

        private void PulseCursorColor()
        {
            if (pulseStage == 0)
            {
                Random rand = new Random();
                if (red > 0.5f)
                    red -= rand.NextFloat(0, 0.4f) * red;
                else
                    red += rand.NextFloat(0, 0.4f) * red;
                if (green > 0.5f)
                    green -= rand.NextFloat(0, 0.4f) * green;
                else
                    green += rand.NextFloat(0, 0.4f) * green;
                if (blue > 0.5f)
                    blue-= rand.NextFloat(0, 0.4f) * blue;
                else
                    blue += rand.NextFloat(0, 0.4f) * blue;
                CapRGBValues();
            }
            pulseStage = (pulseStage + 1) % 10;
        }

        private void CapRGBValues()
        {
            CapValue(ref red);
            CapValue(ref green);
            CapValue(ref blue);
        }

        private void Fade(float r, float g, float b)
        {
            // Float (0 - 1) values converted to 0-255
            int goalBigR = (int)(r * 255);
            int goalBigG = (int)(g * 255);
            int goalBigB = (int)(b * 255);
            int currentBigR = (int)(red * 255);
            int currentBigG = (int)(green * 255);
            int currentBigB = (int)(blue * 255);

            int gapR = CalculateGap(currentBigR, goalBigR);
            int gapG = CalculateGap(currentBigG, goalBigG);
            int gapB = CalculateGap(currentBigB, goalBigB);

            float newR = (currentBigR - gapR) / 255;
            float newG = (currentBigG - gapG) / 255;
            float newB = (currentBigB - gapB) / 255;

            CapValue(ref newR);
            CapValue(ref newG);
            CapValue(ref newB);

            red = newR;
            green = newG;
            blue = newB;
        }

        /// <summary>
        /// Caps between 0 and 1
        /// </summary>
        /// <param name="value"></param>
        private void CapValue(ref float value)
        {
            if (value > 1)
                value = 1;
            else if (value < 0)
                value = 0;
        }

        private int CalculateGap(int currentValue, int goalValue)
        {
            int result = goalValue - currentValue;
            
            // May not be correct math, but my guess...
            // 60 frames = 1 sec
            // 300 frames = 5 sec, i.e. the time to fade to new colour
            if (result > 0)
                result = 300 / result;

            return result;
        }

        public bool Render(float rotation, int mouseX, int mouseY)
        {
            Matrix viewMatrix, projectionMatrix, worldMatrix3D, worldMatrix2D, orthoMatrix;

            // Clear the buffers to begin the scene
            D3D.BeginScene(0.3f, 0, 0.1f, 1.0f);

            // Generate view matrix based on camera's position
            Camera.Render();

            // Get the world, view and projection matrices from the camera and D3D objects
            viewMatrix = Camera.ViewMatrix;
            worldMatrix3D = D3D.WorldMatrix;
            worldMatrix2D = D3D.WorldMatrix;
            projectionMatrix = D3D.ProjectionMatrix;

            // Get ortho matrix
            orthoMatrix = D3D.OrthoMatrix;

            // START 3D rendering (comment out for 2D)
            
            D3D.TurnOffAlphaBlending();
            D3D.TurnZBufferOn(); // Begin 3D rendering
            // Rotate the world matrix by the rotation value (makes model spin)
            Matrix.RotationY(rotation, out worldMatrix3D);

            // Put the model vertex and index buffers on the graphics pipeline to prepare them from drawing
            Model.Render(D3D.DeviceContext);

            // Render the model using the colour shader
            if (!LightShader.Render(D3D.DeviceContext,
                Model.IndexCount,
                worldMatrix3D, viewMatrix, projectionMatrix,
                Model.Texture.TextureResource,
                Light.direction, Light.diffuseColor, Light.ambientColor,
                Light.specularPower, Light.specularColor,
                Camera.GetPosition()))
            {
                MessageBox.Show("Texture shader failed");
                return false;
            }
            

            // START 2D rendering (comment out for 3D / text)
            /*
            D3D.TurnZBufferOff();

            if (!Bitmap.Render(D3D.DeviceContext, 100, 100))
            {
                return false;
            }

            if (!TextureShader.Render(D3D.DeviceContext, Bitmap.IndexCount, worldMatrix, viewMatrix, orthoMatrix, Bitmap.Texture.TextureResource))
                return false;
            */
            // END

            // START 2D text rendering (comment out 2D / 3D)
            D3D.TurnZBufferOff(); // Begin 2D rendering
            D3D.TurnOnAlphaBlending();

            if (!Text.Render(D3D.DeviceContext, worldMatrix2D, orthoMatrix))
                return false;
            // END

            // START 2D cursor rendering
            if (!Cursor.Render(D3D.DeviceContext, worldMatrix2D, orthoMatrix))
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
