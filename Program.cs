using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDXPractice
{
    class Program
    {
        static void Main()
        {
            using (DSystem system = new DSystem())
            {
                system.StartRenderForm("SharpDX Testing");
            }
        }
    }
}
