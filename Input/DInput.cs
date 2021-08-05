using SharpDX;
using SharpDX.DirectInput;
using SharpDXPractice.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpDXPractice.Input
{
    public class DInput
    {
        public string PressedKeys { get; set; } = "";

        #region Input devices
        public DirectInput DirectInput { get; set; }
        public Keyboard Keyboard { get; set; }
        public Mouse Mouse { get; set; }
        #endregion

        #region Input device state
        public KeyboardState KeyboardState;
        public MouseState MouseState;
        public int MouseX { get; set; }
        public int MouseY { get; set; }
        #endregion

        #region Screen properties
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
        #endregion

        internal bool Initialize(DSystemConfiguration config, IntPtr windowHandle)
        {
            // Store screen size; used for positioning mouse cursor
            ScreenWidth = config.Width;
            ScreenHeight = config.Height;

            // Initialize location of mouse on screen
            MouseX = 0;
            MouseY = 0;

            // Initialize interface to Direct Input
            DirectInput = new DirectInput();

            // Initialize DInput interface to keyboard
            Keyboard = new Keyboard(DirectInput);
            Keyboard.Properties.BufferSize = 256;

            /* 
             * Set cooperative level of keyboard to not share with other programs (Exclusive)
             * This means only this application will see the input
             * For other applications to have access to the keyboard while this program is
               running, set it to non-exclusive.
            */
            Keyboard.SetCooperativeLevel(windowHandle, CooperativeLevel.Foreground | CooperativeLevel.Exclusive);

            // Acquire the keyboard
            try
            {
                Keyboard.Acquire();
            }
            catch (SharpDXException Ex)
            {
                MessageBox.Show("Keyboard acquisition failed. \nError: " + Ex.Message);
                return false;
            }

            // Initialize DInput interface to the mouse
            Mouse = new Mouse(DirectInput);
            Mouse.Properties.AxisMode = DeviceAxisMode.Relative;

            // Set cooperative level of the mouse to shware with other programs
            Mouse.SetCooperativeLevel(windowHandle, CooperativeLevel.Foreground | CooperativeLevel.NonExclusive);

            // Acquire the mouse
            try
            {
                Mouse.Acquire();
            }
            catch (SharpDXException Ex)
            {
                MessageBox.Show("Mouse acquisition failed. \nError: " + Ex.Message);
                return false;
            }

            return true;
        }

        public void Shutdown()
        {
            // Release the mouse
            Mouse?.Unacquire();
            Mouse?.Dispose();
            Mouse = null;

            // Release the keyboard
            Keyboard?.Unacquire();
            Keyboard?.Dispose();
            Keyboard = null;

            // Release the main interface to direct input
            DirectInput?.Dispose();
            DirectInput = null;
        }

        // Read the current state of the devices into state buffers we set up
        public bool Frame()
        {
            // Read the current state of the keyboard
            if (!ReadKeyboard())
                return false;

            // Read the current state of the mouse
            if (!ReadMouse())
                return false;

            // Process the changes in the mouse and keyboard
            ProcessInput();

            return true;
        }

        /// <summary>
        /// Read the state of the keyboard into the KeyboardState variable.
        /// The state will show which keys are pressed / not pressed.
        /// It may fail for 5 reasons; this function will account for two:
        /// 1. Focus lost
        /// 2. Keyboard becomes unacquired
        /// If these errors occur, function will Acquire each frame. This
        /// will fail if the window is minimized, but will succeed once the
        /// window is in the foreground again.
        /// </summary>
        /// <returns></returns>
        private bool ReadKeyboard()
        {
            var resultCode = ResultCode.Ok;
            KeyboardState = new KeyboardState();

            try
            {
                Keyboard.GetCurrentState(ref KeyboardState);

                // Create a list of pressed keys
                List<string> pressedKeyList = new List<string>();

                foreach (Key k in KeyboardState.PressedKeys)
                {
                    // Convert the Key enum to the name of the key
                    var enumDisplayStatus = (Key)k;
                    // Add the name as a string to the list
                    pressedKeyList.Add(enumDisplayStatus.ToString());
                }

                // Adds each string from the list to the PressedKeys string,
                // resetting the string if it gets too long.
                foreach (string s in pressedKeyList)
                {
                    if (PressedKeys.Length < 64)
                        PressedKeys += s;
                    else
                    {
                        PressedKeys = "";
                        PressedKeys += s;
                    }
                }
                    
            }
            catch (SharpDXException ex)
            {
                resultCode = ex.Descriptor;
            }

            if (resultCode == ResultCode.InputLost || resultCode == ResultCode.NotAcquired)
                try
                {
                    Keyboard.Acquire();
                }
                catch
                {
                    return true;
                }
            else if (resultCode != ResultCode.Ok)
                return false;

            return true;
        }

        /// <summary>
        /// Same as ReadKeyboard. However, the state of the mouse is
        /// just changes in the position of the mouse from the last
        /// frame, e.g. updates will show that the mouse has moved 5
        /// units to the right, but won't give the actual position of
        /// the mouse on the screen. This is useful for different
        /// purposes, and the position of the mouse on the screen
        /// can be easily maintained regardless.
        /// </summary>
        /// <returns></returns>
        private bool ReadMouse()
        {
            var resultCode = ResultCode.Ok;
            MouseState = new MouseState();

            try
            {
                Mouse.GetCurrentState(ref MouseState);
            }
            catch (SharpDXException ex)
            {
                resultCode = ex.Descriptor;
            }

            if (resultCode == ResultCode.InputLost || resultCode == ResultCode.NotAcquired)
                try
                {
                    Mouse.Acquire();
                }
                catch
                {
                    return true;
                }
            else if (resultCode != ResultCode.Ok)
                return false;

            return true;
        }

        /// <summary>
        /// This method deals with the changes that have happened in
        /// the input devices since the last frame.
        /// 1. Updates mouse location
        /// 2. Check the mouse location never goes off screen
        /// </summary>
        private void ProcessInput()
        {
            // 1.
            MouseX += MouseState.X;
            MouseY += MouseState.Y;

            // 2.
            if (MouseX < 0)
                MouseX = 0;
            if (MouseY < 0)
                MouseY = 0;

            if (MouseX > ScreenWidth)
                MouseX = ScreenWidth;
            if (MouseY > ScreenHeight)
                MouseY = ScreenHeight;
        }

        public bool IsEscapePressed()
        {
            return KeyboardState.PressedKeys != null && KeyboardState.PressedKeys.Contains(Key.Escape);
        }

        public void GetMouseLocation(out int mouseX, out int mouseY)
        {
            mouseX = MouseX;
            mouseY = MouseY;
        }
    }
}
