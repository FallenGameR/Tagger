using System;
using System.Collections.Generic;
using System.Linq;
using Tagger.WinAPI;
using Utils.Extensions;
using Tagger.Wpf.Windows;
using Tagger.ViewModels;
using Tagger.Wpf;
using Microsoft.Practices.Prism.Commands;
using System.Windows.Threading;
using System.Reflection;
using System.ComponentModel;

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
            var hostListner = new WindowListner(hostWindow);
            hostListner.WindowDestroyed += delegate { RegistrationManager.UnregisterTag(hostWindow); };

            var tagRender = new TagViewModel();
            var tagWindow = new TagWindow();

            // Subscriptions
            Action<double> updatePosition = (double width) =>
            {
                var clientArea = WindowSizes.GetClientArea(hostWindow);
                tagWindow.Top = clientArea.Top + tagRender.OffsetTop;
                tagWindow.Left = clientArea.Right - width - tagRender.OffsetRight;
            };
            // client area
            hostListner.ClientAreaChanged += delegate { updatePosition(tagWindow.Width); };
            // text width 
            tagRender.PropertyChanged += delegate { updatePosition(tagWindow.Width); };
            // 
            tagWindow.MouseRightButtonUp += delegate { tagRender.ToggleSettingsCommand.Execute(null); };
            // Restored
            tagWindow.SizeChanged += (sender, args) => updatePosition(args.NewSize.Width);

            // Initialize tag window
            // To update the very first position right after the data binding that would
            // define tag window size but before the render (render has lower priority in dispatcher queue)
            tagWindow.DataContext = tagRender;
            tagWindow.SetOwner(hostWindow);
            tagWindow.Show();
            //tagWindow.Dispatcher.Invoke(updatePosition, DispatcherPriority.DataBind, tagWindow.Width);

            //
            var settingsWindow = new SettingsWindow();
            tagRender.HideSettingsCommand = new DelegateCommand<object>(delegate
            {
                settingsWindow.ToggleVisibility();
                NativeAPI.SetForegroundWindow(hostWindow);
            });
            // Subscriptions
            settingsWindow.Closing += (sender, args) =>
            {
                args.Cancel = true;
                settingsWindow.Hide();
            };
            // Initialize settings window
            settingsWindow.DataContext = tagRender;
            settingsWindow.SetOwner(hostWindow);
            settingsWindow.Show();

            // Perform cross window commands initialization
            tagRender.ToggleSettingsCommand = new DelegateCommand<object>(o => settingsWindow.ToggleVisibility());

            // Create new tag context object 
            var context = new TagContext
            {
                HostWindow = hostWindow,
                HostListner = hostListner,
                TagRender = tagRender,
                TagWindow = tagWindow,
                SettingsWindow = settingsWindow,
            };

            // Store it to manage lifetime
            lock( RegistrationManager.KnownTags )
            {
                RegistrationManager.KnownTags.Add(context);
            }
        }

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
