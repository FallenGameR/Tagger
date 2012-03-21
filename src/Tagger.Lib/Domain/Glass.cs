using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Tagger.WinAPI;
using Utils.Extensions;

namespace Tagger
{
    /// <summary>
    /// Helper class that can make glass-style window out from a regular WPF window
    /// </summary>
    public static class Glass
    {
        /// <summary>
        /// Enable glass-style window
        /// </summary>
        /// <param name="window">Window to be glassified</param>
        public static void Enable(Window window)
        {
            // Check that glass is enabled
            if (!NativeAPI.DwmIsCompositionEnabled())
            {
                return;
            }

            RoutedEventHandler enableAction = delegate
            {
                // Set transparent background
                var handle = window.GetHandle();
                window.Background = Brushes.Transparent;
                HwndSource.FromHwnd(handle).CompositionTarget.BackgroundColor = Colors.Transparent;

                // Extend glass frame into the form
                var margins = new NativeAPI.MARGINS
                {
                    Left = -1,
                    Right = -1,
                    Top = -1,
                    Bottom = -1,
                };

                NativeAPI.DwmExtendFrameIntoClientArea(handle, ref margins);
            };

            // Apply enable action only when we could get form handle
            if (window.IsLoaded)
            {
                enableAction(null, null);
            }
            else
            {
                window.Loaded += enableAction;
            }
        }
    }
}
