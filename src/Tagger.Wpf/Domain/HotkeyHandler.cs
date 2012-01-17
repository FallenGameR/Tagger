using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using ManagedWinapi.Accessibility;
using ManagedWinapi.Windows;
using Tagger.WinAPI.WaitChainTraversal;

namespace Tagger.Wpf
{
    public static class HotkeyHandler
    {
       
        public static void HandleHotkeyPress()
        {
            var m_OverlayWindow = new OverlayWindow();
            m_OverlayWindow.HookToForegroundWindow();
            m_OverlayWindow.Show();
        }
    }
}
