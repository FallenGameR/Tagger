//-----------------------------------------------------------------------
// <copyright file="Windows.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Tagger.WinAPI
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// Win32 API call for windows manipulation
    /// </summary>
    public static partial class NativeMethods
    {
        #region Constants

        /// <summary>
        /// Operation succeeded
        /// </summary>
        /// <remarks>
        /// Win32 error code
        /// </remarks>
        public const int NO_ERROR = 0;

        /// <summary>
        /// Operation succeeded
        /// </summary>
        /// <remarks>
        /// This is an HRESULT
        /// </remarks>
        public const int S_OK = 0;

        /// <summary>
        /// Sets a new window style
        /// </summary>
        /// <remarks>
        /// SetWindowLong function 
        /// </remarks>
        public const int GWL_STYLE = -16;

        /// <summary>
        /// The window is initially visible.
        /// </summary>
        /// <remarks>
        /// Window Styles
        /// </remarks>
        public const ulong WS_VISIBLE = 0x10000000L;

        /// <summary>
        /// The window has a thin-line border.
        /// </summary>
        /// <remarks>
        /// Window Styles
        /// </remarks>
        public const ulong WS_BORDER = 0x00800000L;

        /// <summary>
        /// The WM_NCCALCSIZE message is sent when the size and position of a window's client area 
        /// must be calculated. By processing this message, an application can control the content 
        /// of the window's client area when the size or position of the window changes.
        /// </summary>
        /// <remarks>
        /// WindowProc function
        /// </remarks>
        public const uint WM_NCCALCSIZE = 0x83;

        #endregion

        #region Methods

        /// <summary>
        /// Retrieves the identifier of the thread that created the specified window and, 
        /// optionally, the identifier of the process that created the window. 
        /// </summary>
        /// <param name="hWnd">A handle to the window</param>
        /// <param name="lpdwProcessId">A pointer to a variable that receives the process identifier</param>
        /// <returns>The return value is the identifier of the thread that created the window</returns>
        [DllImport("user32.dll")]
        internal static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        /// <summary>
        /// Retrieves the dimensions of the bounding rectangle of the specified window
        /// </summary>
        /// <param name="hwnd">A handle to the window</param>
        /// <param name="lpRect">A pointer to a RECT structure that receives the screen coordinates of the upper-left and lower-right corners of the window</param>
        /// <returns>If the function succeeds, the return value is nonzero</returns>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        /// <summary>
        /// Sends the specified message to a window 
        /// </summary>
        /// <param name="hWnd">A handle to the window whose window procedure will receive the message</param>
        /// <param name="msg">The message to be sent</param>
        /// <param name="wParam">Additional message-specific information 1</param>
        /// <param name="lParam">Additional message-specific information 2</param>
        /// <returns>The return value specifies the result of the message processing; it depends on the message sent</returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, ref RECT lParam);

        #endregion

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
    }
}
