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
    /// Win32 API call for windows handles manipulation
    /// </summary>
    public static partial class NativeAPI
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

        /// <summary>
        /// An application-defined callback function used with the EnumWindows or EnumDesktopWindows function.
        /// </summary>
        /// <param name="hwnd">A handle to a top-level window.</param>
        /// <param name="lParam">The application-defined value given in EnumWindows or EnumDesktopWindows. </param>
        /// <returns>To continue enumeration, the callback function must return TRUE; to stop enumeration, it must return FALSE. </returns>
        public delegate bool EnumWindowsCallback(IntPtr hwnd, int lParam);

        /// <summary>
        /// Retrieves a handle to the foreground window (the window with which the user is currently working)
        /// </summary>
        /// <returns>
        /// The return value is a handle to the foreground window. The foreground window can 
        /// be NULL in certain circumstances, such as when a window is losing activation.
        /// </returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// Brings the thread that created the specified window into the foreground and activates the window. 
        /// Keyboard input is directed to the window, and various visual cues are changed for the user. The 
        /// system assigns a slightly higher priority to the thread that created the foreground window than 
        /// it does to other threads. 
        /// </summary>
        /// <param name="hWnd">A handle to the window that should be activated and brought to the foreground. </param>
        /// <returns>
        /// If the window was brought to the foreground, the return value is nonzero.
        /// If the window was not brought to the foreground, the return value is zero.
        /// </returns>
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// Retrieves the identifier of the thread that created the specified window and, 
        /// optionally, the identifier of the process that created the window. 
        /// </summary>
        /// <param name="hWnd">A handle to the window</param>
        /// <param name="lpdwProcessId">A pointer to a variable that receives the process identifier</param>
        /// <returns>The return value is the identifier of the thread that created the window</returns>
        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        /// <summary>
        /// Retrieves the dimensions of the bounding rectangle of the specified window
        /// </summary>
        /// <param name="hwnd">A handle to the window</param>
        /// <param name="lpRect">A pointer to a RECT structure that receives the screen coordinates of the upper-left and lower-right corners of the window</param>
        /// <returns>If the function succeeds, the return value is nonzero</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        /// <summary>
        /// Sends the specified message to a window 
        /// </summary>
        /// <param name="hWnd">A handle to the window whose window procedure will receive the message</param>
        /// <param name="msg">The message to be sent</param>
        /// <param name="wParam">Additional message-specific information 1</param>
        /// <param name="lParam">Additional message-specific information 2</param>
        /// <returns>The return value specifies the result of the message processing; it depends on the message sent</returns>
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint msg, int wParam, ref RECT lParam);

        /// <summary>
        /// Retrieves information about the specified window.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.</param>
        /// <param name="nIndex">The zero-based offset to the value to be retrieved.</param>
        /// <returns>
        /// If the function succeeds, the return value is the requested value.
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern ulong GetWindowLong(IntPtr hWnd, int nIndex);

        /// <summary>
        /// Copies the text of the specified window's title bar (if it has one) into a buffer.
        /// </summary>
        /// <param name="hWnd">A handle to the window or control containing the text. </param>
        /// <param name="lpString">The buffer that will receive the text. If the string is as long or longer than the buffer, the string is truncated and terminated with a null character.</param>
        /// <param name="nMaxCount">The maximum number of characters to copy to the buffer, including the null character. If the text exceeds this limit, it is truncated.</param>
        /// <returns>
        /// If the function succeeds, the return value is the length, in characters, of the copied string, not including 
        /// the terminating null character. If the window has no title bar or text, if the title bar is empty, or if the 
        /// window or control handle is invalid, the return value is zero. To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        /// <summary>
        /// Enumerates all top-level windows on the screen by passing the handle to each window, in turn, to an application-defined callback function.
        /// </summary>
        /// <param name="lpEnumFunc">A pointer to an application-defined callback function. For more information, see EnumWindowsProc. </param>
        /// <param name="lParam">An application-defined value to be passed to the callback function. </param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int EnumWindows(EnumWindowsCallback lpEnumFunc, int lParam);

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
