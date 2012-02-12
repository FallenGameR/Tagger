using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Tagger.WinAPI
{
    /// <summary>
    /// Win32 API call for windows handles retrieval
    /// </summary>
    public static partial class NativeAPI
    {
        /// <summary>
        /// Retrieves a handle to the foreground window (the window with which the user is currently working)
        /// </summary>
        /// <returns>The return value is a handle to the foreground window. The foreground window can 
        /// be NULL in certain circumstances, such as when a window is losing activation.</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
    }
}
