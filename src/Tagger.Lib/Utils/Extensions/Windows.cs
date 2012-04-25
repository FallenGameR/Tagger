//-----------------------------------------------------------------------
// <copyright file="Windows.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Utils.Extensions
{
    using System;
    using System.Windows;
    using System.Windows.Interop;

    /// <summary>
    /// Extension class for windows 
    /// </summary>
    public static class Windows
    {
        /// <summary>
        /// Gets native handle corresponding to this tag window
        /// </summary>
        /// <param name="window">Extended window</param>
        /// <returns>Native handle to the window</returns>
        public static IntPtr GetHandle(this Window window)
        {
            return new WindowInteropHelper(window).Handle;
        }

        /// <summary>
        /// Gets native handle for host window (the owner window this tag belongs to)
        /// </summary>
        /// <param name="window">Extended window</param>
        /// <returns>Native handle to the window owner</returns>
        public static IntPtr GetOwner(this Window window)
        {
            return new WindowInteropHelper(window).Owner;
        }

        /// <summary>
        /// Sets native handle for host window (the owner window this tag belongs to)
        /// </summary>
        /// <param name="window">Extended window</param>
        /// <param name="value">Native handle containing new window owner</param>
        public static void SetOwner(this Window window, IntPtr value)
        {
            new WindowInteropHelper(window).Owner = value;
        }

        /// <summary>
        /// Toggles window visibility
        /// </summary>
        /// <param name="window">Extended window</param>
        public static void ToggleVisibility(this Window window)
        {
            if (window.Visibility == Visibility.Visible)
            {
                window.Hide();
            }
            else
            {
                window.Show();
            }
        }
    }
}
