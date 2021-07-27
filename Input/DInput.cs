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
        private Dictionary<Keys, bool> _inputKeys = new Dictionary<Keys, bool>();

        internal void Initialize()
        {
            foreach(Keys key in Enum.GetValues(typeof(Keys)))
            {
                _inputKeys[(Keys)key] = false;
            }
        }

        internal bool IsKeyDown(Keys key)
        {
            return _inputKeys[key];
        }

        internal void KeyDown(Keys key)
        {
            _inputKeys[key] = true;
        }

        internal void KeyUp(Keys key)
        {
            _inputKeys[key] = false;
        }
    }
}
