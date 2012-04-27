//-----------------------------------------------------------------------
// <copyright file="RegistrationManager.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Tagger
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;

    using Tagger.WinAPI;

    using Utils.Diagnostics;
    using Utils.Extensions;

    /// <summary>
    /// Handles registration of tags
    /// </summary>
    public static class RegistrationManager
    {
        /// <summary>
        /// Windows that should not be tagged
        /// </summary>
        private static readonly List<IntPtr> exceptions = new List<IntPtr>();

        /// <summary>
        /// All known tag contexts
        /// </summary>
        private static readonly List<TagContext> knownTags = new List<TagContext>();

        /// <summary>
        /// Template method for tag and settings hotkeys
        /// </summary>
        /// <remarks>
        /// Registers new tag if there was none.
        /// Maintains host window focus.
        /// </remarks>
        /// <param name="tagExistsHandler">Fallback action if tag was already registered</param>
        public static void TagRegistrationHandler(Action<TagContext> tagExistsHandler)
        {
            // Get foremost host
            var host = GetHostHandle();
            if (host == IntPtr.Zero)
            {
                return;
            }

            // Register new tag if there is no tag associated with host window
            var context = GetKnownTagContext(host);
            if (context == null)
            {
                RegisterTag(host);
                return;
            }

            // Otherwise use custom handler for already existing tag
            tagExistsHandler(context);

            if (context.TagControlWindow.IsVisible)
            {
                // Keep focus on settings text if tag control window is visible
                context.TagControlWindow.Focus();
                context.TagControlWindow.TextTxt.Focus();
                context.TagControlWindow.TextTxt.SelectAll();
            }
            else
            {
                // Otherwise keep focus on host window
                NativeAPI.SetForegroundWindow(context.HostWindow);
            }
        }

        /// <summary>
        /// Handles tag global hotkey
        /// </summary>
        public static void TagHotkeyHandler()
        {
            TagRegistrationHandler(context => context.TagOverlayWindow.ToggleVisibility());
        }

        /// <summary>
        /// Handles settings global hotkey
        /// </summary>
        public static void SettingsHotkeyHandler()
        {
            TagRegistrationHandler(context => context.TagControlWindow.ToggleVisibility());
        }

        /// <summary>
        /// Gets all distinct tag labels from all registered tags
        /// </summary>
        /// <returns>Distinct tag labels enumeration</returns>
        /// <remarks>Default tag is excluded since it is not interesting to the user</remarks>
        public static IEnumerable<TagLabel> GetExistingTags()
        {
            return (
                from tag in knownTags
                let label = tag.TagViewModel.GetLabel()
                orderby label.Text
                orderby label.Color.ToString()
                select label).Distinct();
        }

        /// <summary>
        /// Register exception window that should not be tagged
        /// </summary>
        /// <param name="window">Window that should not be tagged</param>
        public static void RegisterException(Window window)
        {
            Check.Require(window != null, "Window should not be null");
            var handle = window.GetHandle();

            Check.Ensure(handle != IntPtr.Zero, "Do not call RegisterException from constructor");
            exceptions.Add(handle);
        }

        /// <summary>
        /// Unregister exception window that should not be tagged
        /// </summary>
        /// <param name="window">Window that should not be tagged</param>
        public static void UnregisterException(Window window)
        {
            Check.Require(window != null, "Window should not be null");
            var handle = window.GetHandle();

            Check.Ensure(handle != IntPtr.Zero, "Do not call UnregisterException from constructor");
            exceptions.Remove(handle);
        }

        /// <summary>
        /// Registeres new tag
        /// </summary>
        /// <param name="hostWindow">Handle to the window host that is tagged</param>
        private static void RegisterTag(IntPtr hostWindow)
        {
            var context = new TagContext();
            context.AttachToHost(hostWindow);
            context.HostWindowListner.WindowDestroyed += delegate { UnregisterTag(hostWindow); };

            lock (knownTags)
            {
                knownTags.Add(context);
            }
        }

        /// <summary>
        /// Unregisters existing tag and clean up associated resources
        /// </summary>
        /// <param name="hostWindow">Handle to the window host that is tagged</param>
        internal static void UnregisterTag(IntPtr hostWindow)
        {
            lock (knownTags)
            {
                var match = knownTags.SingleOrDefault(c => c.HostWindow == hostWindow);
                if (match != null)
                {
                    match.Dispose();
                    knownTags.Remove(match);
                }
            }
        }

        /// <summary>
        /// Unregisters existing tag and clean up associated resources
        /// </summary>
        /// <param name="tagViewModel">Tag view model that is used for tag lookup</param>
        public static void UnregisterTag(TagViewModel tagViewModel)
        {
            lock (RegistrationManager.KnownTags)
            {
                var match = RegistrationManager.KnownTags.SingleOrDefault(c => c.TagViewModel == tagViewModel);
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
            return knownTags.SingleOrDefault(c => c.HostWindow == host);
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

            // If window is in exceptions, ignore call
            var exceptionMatch = exceptions.Contains(foremostWindow);
            if (exceptionMatch)
            {
                return IntPtr.Zero;
            }

            // If tag window is foremost, return its owner
            var tagMatch = knownTags.SingleOrDefault(c => c.TagOverlayWindow.GetHandle() == foremostWindow);
            if (tagMatch != null)
            {
                return tagMatch.TagOverlayWindow.GetOwner();
            }

            // If settings window is foremost, return corresponding tag owner
            var settingsMatch = knownTags.SingleOrDefault(c => c.TagControlWindow.GetHandle() == foremostWindow);
            if (settingsMatch != null)
            {
                return settingsMatch.TagOverlayWindow.GetOwner();
            }

            // Else return foremost window handle
            return foremostWindow;
        }
    }
}
