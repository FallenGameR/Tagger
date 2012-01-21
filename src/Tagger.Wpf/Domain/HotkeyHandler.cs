
using System.Collections.Generic;
using System;
namespace Tagger.Wpf
{
    /// <summary>
    /// Handles pressing of the hotkeys
    /// </summary>
    public static class HotkeyHandler
    {
        private static Dictionary<IntPtr, OverlayWindow> RegisteredTags;

        /// <summary>
        /// Handle just pressed hotkey
        /// </summary>
        public static void HandleHotkeyPress()
        {
            var m_OverlayWindow = new OverlayWindow();
            m_OverlayWindow.HookToForegroundWindow();
            m_OverlayWindow.Show();

            // If tag exist, close it

            // Else create it 

            // Keep settings per window title basis
        }
    }
}
