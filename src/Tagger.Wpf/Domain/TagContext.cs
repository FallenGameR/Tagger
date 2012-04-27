//-----------------------------------------------------------------------
// <copyright file="TagContext.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Tagger
{
    using System;
    using System.ComponentModel;

    using Microsoft.Practices.Prism.Commands;

    using Tagger.Properties;
    using Tagger.WinAPI;
    using Tagger.Wpf;
    using Tagger.Wpf.Windows;

    using Utils.Diagnostics;
    using Utils.Extensions;

    /// <summary>
    /// Context of a tag
    /// </summary>
    /// <remarks>
    /// Thoughts regarding event unsubscription. I've investigated possibility of a memory leak
    /// here and it looks like there should be none. We have several tag-related classes that
    /// subscribe on each other and are referenced from RegistrsationManager as TagContext 
    /// instance. We have two external events we rely on that could possibly store another 
    /// reference on tag related classes:
    /// - AccessibleEventListener that wraps WinAPI Hook and 
    /// - AutomationEventHandler that wraps UI Automation callback. 
    /// <para/>
    /// Both these events are gracefully unsubscribed from in WindowListner class. Plus there 
    /// is an implicit event reference from RegistrsationManager via WindowListner instance
    /// (host destruction subscription). To handle correct unreferencing WindowListner 
    /// unregisters all subscribers in Dispose. 
    /// <para/>
    /// Thus for garbage collection to succeed on tag deletion we need:
    /// - call dispose on WindowListner instance
    /// - unreference tag context from KnownTags collection
    /// <para/>
    /// Here is some additional reading regarding events:
    /// http://www.codeproject.com/Articles/29922/Weak-Events-in-C
    /// http://diditwith.net/PermaLink,guid,aacdb8ae-7baa-4423-a953-c18c1c7940ab.aspx
    /// http://msdn.microsoft.com/en-us/library/ms228976.aspx
    /// </remarks>
    public sealed class TagContext : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TagContext"/> class. 
        /// </summary>
        /// <remarks>
        /// Windows objects are created, initialized and ready to connect to a host window
        /// </remarks>
        public TagContext()
        {
            this.TagViewModel = new TagViewModel();
            this.TagOverlayWindow = new TagOverlayWindow { DataContext = this.TagViewModel };
            this.TagControlWindow = new TagControlWindow { DataContext = this.TagViewModel };

            this.TagViewModel.ToggleSettingsCommand = new DelegateCommand<object>(o => this.TagControlWindow.ToggleVisibility());
            this.TagViewModel.HideSettingsCommand = new DelegateCommand<object>(this.HideSettingsAndRefocus);
            this.TagViewModel.PropertyChanged += (s, e) => this.RedrawTagPosition(this.TagOverlayWindow.Width);

            this.TagOverlayWindow.MouseRightButtonUp += (s, e) => this.TagViewModel.ToggleSettingsCommand.Execute(null);
            this.TagOverlayWindow.SizeChanged += (s, e) => this.RedrawTagPosition(e.NewSize.Width);

            this.TagControlWindow.Closing += this.SettingsClosingHandler;
        }

        /// <summary>
        /// Gets tag render view model that is shared between settings and tag windows
        /// </summary>
        public TagViewModel TagViewModel { get; private set; }

        /// <summary>
        /// Gets tag overlay window - the displayed tag itself
        /// </summary>
        public TagOverlayWindow TagOverlayWindow { get; private set; }

        /// <summary>
        /// Gets tag control window - tag appearance settings and other control elements
        /// </summary>
        public TagControlWindow TagControlWindow { get; private set; }

        /// <summary>
        /// Gets window handle that is tagged
        /// </summary>
        public IntPtr HostWindow { get; private set; }

        /// <summary>
        /// Gets listner for events that happens in host process
        /// </summary>
        /// <remarks>
        /// On dispose unregisters all handlers from all its events. That's an unusual 
        /// behaviour but it allows to garbage collect tag context without additional clutter.
        /// </remarks>
        public WindowListner HostWindowListner { get; private set; }

        /// <summary>
        /// Clean up all resources
        /// </summary>
        public void Dispose()
        {
            // Cancel hide on close for settings window
            this.TagControlWindow.Closing -= this.SettingsClosingHandler;

            // Hide and then close both views
            this.TagControlWindow.Dispatcher.Invoke((Action)(() => this.TagControlWindow.Hide()));
            this.TagOverlayWindow.Dispatcher.Invoke((Action)(() => this.TagOverlayWindow.Hide()));
            this.TagOverlayWindow.Dispatcher.Invoke((Action)(() => this.TagOverlayWindow.Close()));
            this.TagControlWindow.Dispatcher.Invoke((Action)(() => this.TagControlWindow.Close()));

            // Dispose underlying view model
            this.TagViewModel.Dispose();

            // Dispose window listner
            if (this.HostWindowListner != null)
            {
                this.HostWindowListner.Dispose();
            }
        }

        /// <summary>
        /// Attach tag to a host window
        /// </summary>
        /// <param name="hostWindow">Window handle to the host window</param>
        /// <remarks>
        /// Checks ensure that this method is called only once per tag context
        /// </remarks>
        public void AttachToHost(IntPtr hostWindow)
        {
            Check.Require(hostWindow != IntPtr.Zero, "Injected host window must not be zero");
            Check.Require(this.HostWindow == default(IntPtr), "Host window must not be initialized");

            // Randomize color if global color randomization setting is set
            if (Settings.Default.GlobalSettings_UseColorRandomization)
            {
                this.TagViewModel.Color = ColorRandom.Next();
            }

            this.HostWindow = hostWindow;

            this.TagOverlayWindow.SetOwner(this.HostWindow);
            this.TagOverlayWindow.Show();

            this.TagControlWindow.SetOwner(this.HostWindow);
            this.TagControlWindow.Show();

            this.HostWindowListner = new WindowListner(this.HostWindow);
            this.HostWindowListner.ClientAreaChanged += (s, e) => this.RedrawTagPosition(this.TagOverlayWindow.Width);
        }

        /// <summary>
        /// Hide settings window instead of close
        /// </summary>
        /// <param name="sender">The parameter is not used</param>
        /// <param name="ea">Closing event args that allow to cancel closing</param>
        /// <remarks>
        /// To actually close the settings window we need to unsubscribe from such handler
        /// </remarks>
        private void SettingsClosingHandler(object sender, CancelEventArgs ea)
        {
            ea.Cancel = true;
            this.TagControlWindow.Hide();
        }

        /// <summary>
        /// Redraw tag window position when it's needed
        /// </summary>
        /// <param name="tagWindowWidth">Width of the tag window</param>
        private void RedrawTagPosition(double tagWindowWidth)
        {
            // Ignore unrelevant updates
            if (this.HostWindow == IntPtr.Zero)
            {
                return;
            }

            // Get client area with guard against lag in receiving windows destroy event
            NativeMethods.RECT clientArea;
            try
            {
                clientArea = WindowSizes.GetClientArea(this.HostWindow);
            }
            catch (Win32Exception)
            {
                return;
            }

            // Calculate new tag position
            var newTop = clientArea.Top + this.TagViewModel.OffsetTop;
            var newLeft = clientArea.Right - tagWindowWidth - this.TagViewModel.OffsetRight;

            // Update position only if coordinates changed to speed up redraw process
            // Redraw would actually happen in WPF redraw step in GUI process (WPF uses 
            // prioritized message processing queue - modification of a data bound parameter 
            // will eventually lead to redraw)
            if (this.TagOverlayWindow.Top != newTop)
            {
                this.TagOverlayWindow.Top = newTop;
            }

            if (this.TagOverlayWindow.Left != newLeft)
            {
                this.TagOverlayWindow.Left = newLeft;
            }
        }

        /// <summary>
        /// Hide settings window and make host window focused
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        private void HideSettingsAndRefocus(object sender)
        {
            if (this.HostWindow == IntPtr.Zero)
            {
                return;
            }

            this.TagControlWindow.ToggleVisibility();
            NativeMethods.SetForegroundWindow(this.HostWindow);
        }
    }
}
