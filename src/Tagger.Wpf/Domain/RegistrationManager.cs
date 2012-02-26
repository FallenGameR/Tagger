﻿using System;
using System.Collections.Generic;
using System.Linq;
using Tagger.WinAPI;
using Utils.Extensions;
using Tagger.Wpf.Windows;
using Tagger.ViewModels;
using Tagger.Wpf;

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
        /// Registeres new tag
        /// </summary>
        /// <param name="hostWindow">Window host that is tagged</param>
        private static void RegisterTag(IntPtr hostWindow)
        {
            // Create tag context objects
            var hostListner = new ProcessListner(hostWindow);
            hostListner.Destroyed += delegate { RegistrationManager.UnregisterTag(hostWindow); };

            var tagRender = new TagRender();
            var tagWindow = new TagWindow();
            var tagModel = new TagModel(tagWindow, hostWindow, tagRender, hostListner);
            tagWindow.Show();

            var settingsWindow = new SettingsWindow();
            var settingsModel = new SettingsModel(settingsWindow, hostWindow, tagRender);
            settingsWindow.Show();

            // Perform settings window injection
            // NOTE: That way we can show the tag as fast as possible (that is good for perceived responsiveness)
            //       and do not need to switch foreground window manually when settings window would appear
            tagModel.SettingsWindow = settingsWindow;

            // Create new tag context object 
            var context = new TagContext
            {
                HostWindow = hostWindow,
                HostListner = hostListner,
                TagRender = tagRender,
                TagWindow = tagWindow,
                TagModel = tagModel,
                SettingsWindow = settingsWindow,
                SettingsModel = settingsModel,
            };

            // Store it to manage lifetime
            lock( RegistrationManager.KnownTags )
            {
                RegistrationManager.KnownTags.Add(context);
            }
        }

        private static void UnregisterTag(IntPtr hostWindow)
        {
            // TODO: Figure out how to get dispose event securelly
            //lock( RegistrationManager.KnownTags )
            //{
            //    var match = RegistrationManager.KnownTags.SingleOrDefault(c => c.HostWindow == hostWindow);
            //    if( match != null )
            //    {
            //        match.Dispose();
            //        RegistrationManager.KnownTags.Remove(match);
            //    }
            //}
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
