using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Tagger.WinAPI;
using Utils.Diagnostics;

namespace Tagger
{
    /// <summary>
    /// Helper class to determine window sizes
    /// </summary>
    public static class WindowSizes
    {
        /// <summary>
        /// Gets host window client area rectangle
        /// </summary>
        /// <param name="windowHandle">Handl to the window that we are interested in</param>
        /// <returns>
        /// Rectangle used to render host window content
        /// </returns>
        public static Tagger.WinAPI.NativeAPI.RECT GetClientArea(IntPtr windowHandle)
        {
            // Get window rectange
            Tagger.WinAPI.NativeAPI.RECT sizes;
            bool success = NativeAPI.GetWindowRect(windowHandle, out sizes);
            if (!success)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            // For the window rectangle determine actual client area
            var zero = NativeAPI.SendMessage(windowHandle, NativeAPI.WM_NCCALCSIZE, 0, ref sizes);
            Check.Ensure(zero == 0);
            return sizes;
        }
    }
}
