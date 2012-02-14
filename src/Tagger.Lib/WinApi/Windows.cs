using System;
using System.Runtime.InteropServices;

namespace Tagger.WinAPI
{
    /// <summary>
    /// Win32 API call for windows handles manipulation
    /// </summary>
    public static partial class NativeAPI
    {
        /// <summary>
        /// The WM_NCCALCSIZE message is sent when the size and position of a window's client area 
        /// must be calculated. By processing this message, an application can control the content 
        /// of the window's client area when the size or position of the window changes.
        /// </summary>
        public static uint WM_NCCALCSIZE = 0x83;

        #region RECT struct

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        #endregion

        // TODO: Do we need to handle LastError codes here?

        /// <summary>
        /// Retrieves a handle to the foreground window (the window with which the user is currently working)
        /// </summary>
        /// <returns>The return value is a handle to the foreground window. The foreground window can 
        /// be NULL in certain circumstances, such as when a window is losing activation.</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// Retrieves the identifier of the thread that created the specified window and, 
        /// optionally, the identifier of the process that created the window. 
        /// </summary>
        /// <param name="hWnd">A handle to the window.</param>
        /// <param name="lpdwProcessId">A pointer to a variable that receives the process identifier.</param>
        /// <returns>The return value is the identifier of the thread that created the window.</returns>
        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, UInt32 msg, Int32 wParam, ref RECT lParam);
    }
}
