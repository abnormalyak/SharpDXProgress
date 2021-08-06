using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXPractice.System.Performance
{
    public class DTimer
    {
        private Stopwatch _stopWatch;
        private float _ticksPerMs;
        private long _lastFrameTime = 0;

        public float FrameTime { get; private set; }
        public float CumulativeFrameTime { get; private set; }

        public bool Initialize()
        {
            // Check if the system supports high performance timers
            if (!Stopwatch.IsHighResolution)
                return false;
            if (Stopwatch.Frequency == 0)
                return false;

            // Calculate how many times the frequency counter ticks / millisecond
            _ticksPerMs = (float)(Stopwatch.Frequency / 1000.0f);

            _stopWatch = Stopwatch.StartNew();
            return true;
        }

        public void Frame()
        {
            // Get current time
            long currentTime = _stopWatch.ElapsedTicks;

            // Calculate the difference in time since last query for current time
            float timeDifference = currentTime - _lastFrameTime;

            // Calculate frame time by time diff over timer speed resolution
            FrameTime = timeDifference / _ticksPerMs;
            CumulativeFrameTime += FrameTime;

            // Record this frame's duration to the last frame time for next frame processing
            _lastFrameTime = currentTime;
        }
    }
}
