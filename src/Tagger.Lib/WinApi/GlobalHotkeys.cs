﻿//-----------------------------------------------------------------------
// <copyright file="GlobalHotkeys.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Tagger.WinAPI
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Win32 API call for hotkey registration
    /// </summary>
    /// <remarks>
    /// Looks like the only way to figure out what process did install a hotkey
    /// is to write a driver - http://www.wasm.ru/article.php?article=gui_subsystem
    /// </remarks>
    public static partial class NativeAPI
    {
        /// <summary>
        /// Changes the hotkey behavior so that the keyboard auto-repeat does not yield multiple hotkey notifications
        /// </summary>
        /// <remarks>
        /// This flag works only starting Windows 7
        /// </remarks>
        public const uint NoRepeat = 0x4000;

        /// <summary>
        /// Window message for global windows hotkey
        /// </summary>
        public const int WM_HOTKEY = 0x0312;

        /// <summary>
        /// Defines a system-wide hot key
        /// </summary>
        /// <param name="hWnd">
        /// A handle to the window that will receive WM_HOTKEY messages generated by the hot key
        /// </param>
        /// <param name="id">
        /// The identifier of the hot key withing a window
        /// </param>
        /// <param name="fsModifiers">
        /// Key modifiers to be used in hotkey, see ModifierKeys enum for details
        /// </param>
        /// <param name="vk">
        /// The virtual-key code of the hot key
        /// </param>
        /// <returns>
        /// true if the function succeeds
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        /// <summary>
        /// Frees a hot key previously registered by the calling thread
        /// </summary>
        /// <param name="hWnd">
        /// A handle to the window associated with the hot key to be freed
        /// </param>
        /// <param name="id">
        /// The identifier of the hot key to be freed
        /// </param>
        /// <returns>
        /// true if the function succeeds
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}