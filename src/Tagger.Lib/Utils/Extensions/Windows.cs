using System;
using System.Windows;
using System.Windows.Interop;

namespace Utils.Extensions
{
    public static class Windows
    {
        /// <summary>
        /// Gets native handle corresponding to this tag window
        /// </summary>
        public static IntPtr GetHandle(this Window window)
        {
            return new WindowInteropHelper(window).Handle;
        }

        /// <summary>
        /// Gets native handle for host window (the owner window this tag belongs to)
        /// </summary>
        public static IntPtr GetOwner(this Window window)
        {
            return new WindowInteropHelper(window).Owner;
        }

        /// <summary>
        /// Sets native handle for host window (the owner window this tag belongs to)
        /// </summary>
        public static void SetOwner(this Window window, IntPtr value)
        {
            new WindowInteropHelper(window).Owner = value;
        }

        /// <summary>
        /// Toggles window visibility
        /// </summary>
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
