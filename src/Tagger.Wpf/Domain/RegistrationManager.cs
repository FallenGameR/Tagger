using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Tagger.Wpf.ViewModels;
using Tagger.WinAPI;
using Tagger.Wpf.Windows;

namespace Tagger.Wpf
{
    /// <summary>
    /// Handles registration of tags
    /// </summary>
    public static class RegistrationManager
    {
        /// <summary>
        /// Dictionary of host window handles and corresponding tag windows
        /// </summary>
        private static Dictionary<IntPtr, TagWindow> KnownTags = new Dictionary<IntPtr, TagWindow>();

        /// <summary>
        /// Handles captured global windows hotkey
        /// </summary>
        public static void GlobalHotkeyHandle()
        {
            var host = GetHostHandle();

            if (RegistrationManager.KnownTags.ContainsKey(host))
            {
                RegistrationManager.ToggleTagVisibility(host);
            }
            else
            {
                RegistrationManager.RegisterNewTag(host);
            }
        }

        /// <summary>
        /// Register new tag window
        /// </summary>
        /// <param name="host">Host for tag window</param>
        private static void RegisterNewTag(IntPtr host)
        {
            //var settings = new SettingsWindow
            //{
            //    DataContext = new SettingsModel(0),
            //};
            //settings.ShowDialog();

            var tag = new TagWindow(host)
            {
                DataContext = new TagModel
                {
                    Text = "File browser",
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize = 50
                }
            };
            tag.Show();
            tag.UpdateLocation();

            RegistrationManager.KnownTags[host] = tag;
            // Preserve settings on per window title basis
            // Populate setting from the preserved state
        }

        /// <summary>
        /// Toggles tag window visibility for already known tag window
        /// </summary>
        /// <param name="host">Host for tag window</param>
        private static void ToggleTagVisibility(IntPtr host)
        {
            if (RegistrationManager.KnownTags[host].Visibility == Visibility.Visible)
            {
                RegistrationManager.KnownTags[host].Hide();
            }
            else
            {
                RegistrationManager.KnownTags[host].Show();
            }
        }

        /// <summary>
        /// Gets handle of the host window
        /// </summary>
        /// <returns>
        /// If foreground window is not tag, method would return foreground window.
        /// If foreground window is registered tag, method would return its host.
        /// </returns>
        private static IntPtr GetHostHandle()
        {
            var foremostWindow = NativeAPI.GetForegroundWindow();
            var existingTagHost = RegistrationManager.KnownTags.Keys.SingleOrDefault(
                host => foremostWindow == RegistrationManager.KnownTags[host].Handle);

            return (existingTagHost == IntPtr.Zero)
                ? foremostWindow
                : existingTagHost;
        }
    }
}
