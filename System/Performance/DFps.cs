using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXPractice.System.Performance
{
    public class DFps
    {
        public int Fps { get; private set; }
        private int Count;
        private TimeSpan StartTime;

        public void Initialize()
        {
            Fps = 0;
            Count = 0;
            StartTime = DateTime.Now.TimeOfDay;
        }

        public void Frame()
        {
            // The number of frames passed this second
            Count++;

            // Check if a second has passed
            int secondsPassed = (DateTime.Now.TimeOfDay - StartTime).Seconds;
            
            // If a second has passed, update the fps value based on Count,
            // and reset other values.
            if (secondsPassed >= 1)
            {
                Fps = Count;

                Count = 0;
                StartTime = DateTime.Now.TimeOfDay;
            }
        }
    }
}
