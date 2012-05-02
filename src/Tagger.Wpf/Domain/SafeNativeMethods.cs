//-----------------------------------------------------------------------
// <copyright file="SafeNativeMethods.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Tagger
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Win32 API call for windows manipulation
    /// </summary>
    public partial class SafeNativeMethods
    {
        /// <summary>
        /// Retrieves a handle to the foreground window (the window with which the user is currently working)
        /// </summary>
        /// <returns>
        /// The return value is a handle to the foreground window. The foreground window can 
        /// be NULL in certain circumstances, such as when a window is losing activation.
        /// </returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

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
        internal static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
