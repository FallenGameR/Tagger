using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Tagger.Wpf.ViewModels;
using Tagger.WinAPI;

namespace Tagger.Wpf
{
    /// <summary>
    /// Handles pressing of the hotkeys
    /// </summary>
    public static class HotkeyHandler
    {
        private static Dictionary<IntPtr, TagWindow> RegisteredTags = new Dictionary<IntPtr, TagWindow>();
        private static HashSet<IntPtr> TagHandles = new HashSet<IntPtr>();

        private static IntPtr GetHandle()
        {
            var foreground = NativeAPI.GetForegroundWindow();

            if (TagHandles.Contains(foreground))
            {
                return RegisteredTags.Keys.Single(host => new WindowInteropHelper(RegisteredTags[host]).Handle == foreground);
            }
            else
            {
                return foreground;
            }
        }


        /// <summary>
        /// Handle just pressed hotkey
        /// </summary>
        public static void HandleHotkeyPress()
        {
            // Keep on per handle basis
            var host = GetHandle();

            if (RegisteredTags.ContainsKey(host))
            {
                // If tag exist, close it
                if (RegisteredTags[host].Visibility == Visibility.Visible)
                {
                    RegisteredTags[host].Hide();
                }
                else
                {
                    RegisteredTags[host].Show();
                }
            }
            else
            {
                // Else create it 
                var tag = new TagWindow(host)
                {
                    DataContext = new TagViewModel
                    {
                        Text = "File browser",
                        FontFamily = new FontFamily("Segoe UI"),
                        FontSize = 50
                    }
                };
                RegisteredTags[host] = tag;
                TagHandles.Add( new WindowInteropHelper(tag).Handle );
            }

            // Preserve settings on per window title basis
            // Populate setting from the preserved state
        }
    }
}
