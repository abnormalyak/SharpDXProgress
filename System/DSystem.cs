using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.Windows;
using SharpDXPractice.Input;
using SharpDXPractice.Graphics;
using SharpDXPractice.System;
using SharpDXPractice.Sound;
using SharpDXPractice.System.Performance;

namespace SharpDXPractice
{
    public class DSystem                    // 120 lines
    {
        // Properties
        private RenderForm RenderForm { get; set; }
        public DSystemConfiguration Configuration { get; private set; }
        public DInput Input { get; private set; }
        public DGraphics Graphics { get; private set; }
        public DSound Sound { get; private set; }
        public DFps Fps { get; private set; }
        public DCpu Cpu { get; private set; }
        public DTimer Timer { get; private set; }

        // Constructor
        public DSystem() { }

        public static void StartRenderForm(string title, int width, int height, bool vSync, bool fullScreen = true)
        {
            DSystem system = new DSystem();
            system.Initialize(title, width, height, vSync, fullScreen);
            system.RunRenderForm();
        }

        // Methods
        public virtual bool Initialize(string title, int width, int height, bool vSync, bool fullScreen)
        {
            bool result = false;

            if (Configuration == null)
                Configuration = new DSystemConfiguration(title, width, height, fullScreen, vSync);

            // Initialize Window.
            InitializeWindows(title);

            if (Input == null)
            {
                Input = new DInput();
                if (!Input.Initialize(Configuration, RenderForm.Handle))
                    return false;
            }
            if (Graphics == null)
            {
                Graphics = new DGraphics();
                result = Graphics.Initialize(Configuration, RenderForm.Handle);
            }
            #region Sound
            //if (Sound == null)
            //{
            //    Sound = new DSound("sound01.wav");

            //    if (!Sound.Initialize(RenderForm.Handle))
            //    {
            //        MessageBox.Show("Could not initialize Direct Sound.");
            //        return false;
            //    }
            //}

            //Sound.PlayWavFile(5);
            #endregion

            #region Performance
            Fps = new DFps();
            Fps.Initialize();

            Cpu = new DCpu();
            Cpu.Initialize();

            Timer = new DTimer();
            if (!Timer.Initialize())
            {
                MessageBox.Show("Could not initialize the timer object.");
                return false;
            }
            #endregion

            return result;
        }
        private void InitializeWindows(string title)
        {
            int width = Screen.PrimaryScreen.Bounds.Width;
            int height = Screen.PrimaryScreen.Bounds.Height;

            // Initialize Window.
            RenderForm = new RenderForm(title)
            {
                ClientSize = new Size(Configuration.Width, Configuration.Height),
                FormBorderStyle = DSystemConfiguration.BorderStyle
            };

            // The form must be showing in order for the handle to be used in Input and Graphics objects.
            RenderForm.Show();
            RenderForm.Location = new Point((width / 2) - (Configuration.Width / 2), (height / 2) - (Configuration.Height / 2));
        }
        private void RunRenderForm()
        {
            RenderLoop.Run(RenderForm, () =>
            {
                if (!Frame())
                    ShutDown();
            });
        }
        public bool Frame()
        {
            // Check if the user pressed escape and wants to exit the application.
            if (!Input.Frame() || Input.IsEscapePressed())
                return false;

            // Performance stats
            Cpu.Frame();
            Fps.Frame();
            Timer.Frame();

            // Update the Graphics class with the location of the mouse
            int mouseX, mouseY;
            Input.GetMouseLocation(out mouseX, out mouseY);

            // Do the frame processing for the graphics object
            if (!Graphics.Frame(mouseX, mouseY, Input.PressedKeys, Fps.Fps, Cpu.CpuUsage, Timer.FrameTime))
                return false;

            // Render the graphics to the screen
            if (!Graphics.Render(DGraphics.rotation, mouseX, mouseY))
                return false;

            return true;
        }

        public void ShutDown()
        {
            ShutdownWindows();

            Timer = null;
            
            Cpu?.Shutdown();
            Cpu = null;

            Fps = null;

            Graphics?.ShutDown();
            Graphics = null;

            Input?.Shutdown();
            Input = null;

            Configuration = null;
        }
        private void ShutdownWindows()
        {
            RenderForm?.Dispose();
            RenderForm = null;
        }
    }
}