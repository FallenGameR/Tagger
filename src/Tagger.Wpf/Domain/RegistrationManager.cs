using System;
using System.Collections.Generic;
using System.Linq;
using Tagger.WinAPI;
using Utils.Extensions;

namespace Tagger
{
    /// <summary>
    /// Handles registration of tags
    /// </summary>
    public static class RegistrationManager
    {
        /// <summary>
        /// All known tag contexts
        /// </summary>
        private static List<TagContext> KnownTags = new List<TagContext>();

        /// <summary>
        /// Handles captured global windows hotkey
        /// </summary>
        public static void GlobalHotkeyHandle()
        {
            // Get foremost host
            var host = RegistrationManager.GetHostHandle();
            if (host == IntPtr.Zero) { return; }

            // Togle visibility if there is a tag for such host already registered
            var context = RegistrationManager.GetKnownTagContext(host);
            if (context != null)
            {
                context.TagWindow.ToggleVisibility();
                NativeAPI.SetForegroundWindow(host);
                return;
            }

            // Register new tag
            RegistrationManager.RegisterTag(host);
        }

        /// <summary>
        /// Gets all distinct tag labels from all registered tags
        /// </summary>
        /// <returns>Distinct tag labels enumeration</returns>
        /// <remarks>Default tag is excluded since it is not interesting to the user</remarks>
        public static IEnumerable<TagLabel> GetExistingTags()
        {
            var defaultLabel = new TagLabel(new TagViewModel());
            var distinct = RegistrationManager.KnownTags.Select(c => c.TagViewModel.GetLabel()).Distinct();
            return
                from label in distinct
                where !label.Equals(defaultLabel)
                orderby label.Text
                orderby label.Color.ToString()
                select label;
        }

        /// <summary>
        /// Registeres new tag
        /// </summary>
        /// <param name="hostWindow">Handle to the window host that is tagged</param>
        private static void RegisterTag(IntPtr hostWindow)
        {
            var context = new TagContext();
            context.AttachToHost(hostWindow);
            context.HostWindowListner.WindowDestroyed += delegate { RegistrationManager.UnregisterTag(hostWindow); };

            lock( RegistrationManager.KnownTags )
            {
                RegistrationManager.KnownTags.Add(context);
            }
        }

        /// <summary>
        /// Unregisters existing tag and clean it up
        /// </summary>
        /// <param name="hostWindow">Handle to the window host that is tagged</param>
        private static void UnregisterTag(IntPtr hostWindow)
        {
            lock (RegistrationManager.KnownTags)
            {
                var match = RegistrationManager.KnownTags.SingleOrDefault(c => c.HostWindow == hostWindow);
                if (match != null)
                {
                    match.Dispose();
                    RegistrationManager.KnownTags.Remove(match);
                }
            }
        }

        /// <summary>
        /// Find tag context based on host window handle
        /// </summary>
        /// <param name="host">Window handle of the tagged window (the host)</param>
        /// <returns>Existing context or null</returns>
        private static TagContext GetKnownTagContext(IntPtr host)
        {
            return RegistrationManager.KnownTags.SingleOrDefault(c => c.HostWindow == host);
        }

        /// <summary>
        /// Gets handle of the host window based on the foremost window
        /// </summary>
        /// <returns>
        /// Host window handle; handles situation when tag or settings window are in the front
        /// </returns>
        private static IntPtr GetHostHandle()
        {
            var foremostWindow = NativeAPI.GetForegroundWindow();

            // If tag window is foremost, return its owner
            var tagMatch = RegistrationManager.KnownTags.SingleOrDefault(c => c.TagWindow.GetHandle() == foremostWindow);
            if (tagMatch != null)
            {
                return tagMatch.TagWindow.GetOwner();
            }

            // If settings window is foremost, return corresponding tag owner
            var settingsMatch = RegistrationManager.KnownTags.SingleOrDefault(c => c.SettingsWindow.GetHandle() == foremostWindow);
            if (settingsMatch != null)
            {
                return settingsMatch.TagWindow.GetOwner();
            }

            // Else return foremost window handle
            return foremostWindow;
        }
    }
}
