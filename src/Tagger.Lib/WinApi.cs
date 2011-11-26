using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Tagger.Lib
{
    public static class WinApi
    {
        public static void Test( int pid )
        {
            var process = Process.GetProcessById( pid );
            var tids = process.Threads.Cast<ProcessThread>().Select( t => t.Id );
          
        }
    }
}
