//-----------------------------------------------------------------------
// <copyright file="WindowSizes.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Tagger
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using Tagger.WinAPI;

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
        public static NativeAPI.RECT GetClientArea(IntPtr windowHandle)
        {
            // Get window rectange
            NativeAPI.RECT sizes;
            bool success = NativeAPI.GetWindowRect(windowHandle, out sizes);
            if (!success)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            // For the window rectangle determine actual client area
            // NOTE: Return code is not checked since WPF glass applications return something weird in it
            NativeAPI.SendMessage(windowHandle, NativeAPI.WM_NCCALCSIZE, 0, ref sizes);
            return sizes;
        }
    }
}
