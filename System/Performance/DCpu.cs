using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXPractice.System.Performance
{
    public class DCpu
    {
        private bool CanReadCpu;
        private PerformanceCounter Counter;
        private TimeSpan LastSampleTime;
        private long _cpuUsage;

        public int CpuUsage 
        { 
            get 
            {
                return CanReadCpu ? (int)_cpuUsage : 0;
            } 
        }

        public void Initialize()
        {
            CanReadCpu = true;

            try
            {
                Counter = new PerformanceCounter();
                Counter.CategoryName = "Processor";
                Counter.CounterName = "% Processor Time";
                Counter.InstanceName = "_Total";

                LastSampleTime = DateTime.Now.TimeOfDay;

                _cpuUsage = 0;
            }
            catch
            {
                CanReadCpu = false;
            }
        }

        public void Shutdown()
        {
            if (CanReadCpu)
                Counter.Close();
        }

        public void Frame()
        {
            if (CanReadCpu)
            {
                int secondsPassed = (DateTime.Now.TimeOfDay - LastSampleTime).Seconds;

                if (secondsPassed >= 1)
                {
                    LastSampleTime = DateTime.Now.TimeOfDay;
                    _cpuUsage = (int)Counter.NextValue();
                }
            }
        }
    }
}
