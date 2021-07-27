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


namespace SharpDXPractice
{
    public class DSystem : IDisposable
    {
        private RenderForm _renderForm { get; set; }
        public int Width = 1280;
        public int Height = 720;
        public DInput Input { get; private set; }
        public DGraphics Graphics { get; private set; }

        public DSystem() { }

        public void StartRenderForm(string title)
        {
            Initialize(title);
            RunRenderForm();

        }

        public virtual void Initialize(string title)
        {
            InitializeWindows(title);

            _renderForm.BackColor = Color.Black;

            if (Input == null)
            {
                Input = new DInput();
                Input.Initialize();
            }

            if (Graphics == null)
            {
                Graphics = new DGraphics();
            }
        }

        private void InitializeWindows(string title)
        {
            int width = Screen.PrimaryScreen.Bounds.Width;
            int height = Screen.PrimaryScreen.Bounds.Height;

            _renderForm = new RenderForm(title)
            {
                ClientSize = new System.Drawing.Size(Width, Height)
            };

            _renderForm.Show();
            _renderForm.Location = new Point(width / 2, height / 2);
        }

        private void RunRenderForm()
        {
            _renderForm.KeyDown += (s, e) => Input.KeyDown(e.KeyCode);
            _renderForm.KeyUp += (s, e) => Input.KeyUp(e.KeyCode);

            RenderLoop.Run(_renderForm, () =>
            {
                if (!Frame())
                    ShutDown();
            });
        }

        public bool Frame()
        {
            if (Input.IsKeyDown(Keys.Escape))
                return false;

            return Graphics.Frame();
        }

        public void ShutDown()
        {
            Dispose();

            Graphics?.ShutDown();
            Graphics = null;
            Input = null;
        }

        public void Dispose()
        {
            _renderForm?.Dispose();
            _renderForm = null;
        }
    }
}
