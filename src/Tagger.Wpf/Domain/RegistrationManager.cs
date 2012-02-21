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
            var host = RegistrationManager.GetHostHandle();
            var context = RegistrationManager.GetKnownTagContext(host);

            if (context != null)
            {
                context.ToggleTagVisibility();
            }
            else
            {
                context = new TagContext(host);
                RegistrationManager.KnownTags.Add(context);
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

        // TODO: Preserve settings on per window title basis, populate setting from the preserved state
    }
}
