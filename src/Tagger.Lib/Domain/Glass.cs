using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Tagger.WinAPI;

namespace Tagger
{
    public static class Glass
    {
        public static void Enable(Window window)
        {
            if (!NativeAPI.DwmIsCompositionEnabled()) { return; }

            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero)
                throw new InvalidOperationException("The Window must be shown before extending glass.");

            window.Background = Brushes.Transparent;
            HwndSource.FromHwnd(hwnd).CompositionTarget.BackgroundColor = Colors.Transparent;

            var margins = new NativeAPI.MARGINS
            {
                Left = -1,
                Right = -1,
                Top = -1,
                Bottom = -1,
            };
            NativeAPI.DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }
    }
}
