// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TagOverlayWindow.xaml.cs" company="none">
//   Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Tagger.Wpf
{
    using System;
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// The tag overlay window.
    /// </summary>
    /// <remarks>
    /// Original code for drag and drop:
    /// http://groups.google.com/group/wpf-disciples/browse_thread/thread/ece72e5eb7f6c217?pli=1
    /// </remarks>
    public sealed partial class TagOverlayWindow : Window, IDisposable
    {
        /// <summary>
        /// The last known mouse location.
        /// </summary>
        private Point? lastKnownMouseLocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagOverlayWindow"/> class.
        /// </summary>
        public TagOverlayWindow()
        {
            this.InitializeComponent();

            this.PreviewMouseMove += this.Window_PreviewMouseMove;
            this.PreviewMouseUp += this.Window_PreviewMouseUp;
            this.LocationChanged += this.Window_LocationChanged;
            this.PreviewMouseDown += this.Window_PreviewMouseDown;
        }

        /// <summary>
        /// Gets TagViewModel.
        /// </summary>
        private TagViewModel TagViewModel
        {
            get
            {
                return (TagViewModel)this.DataContext;
            }
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.PreviewMouseMove -= this.Window_PreviewMouseMove;
            this.PreviewMouseUp -= this.Window_PreviewMouseUp;
            this.LocationChanged -= this.Window_LocationChanged;
            this.PreviewMouseDown -= this.Window_PreviewMouseDown;
        }

        /// <summary>
        /// The window location changed handler.
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            this.lastKnownMouseLocation = this.PointToScreen(Mouse.GetPosition(this));
        }

        /// <summary>
        /// The window preview mouse down handler.
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.lastKnownMouseLocation = this.PointToScreen(Mouse.GetPosition(this));
        }

        /// <summary>
        /// Drag tag via offset property modification
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            var captured = this.CaptureMouse();
            if (!captured)
            {
                return;
            }

            Point currentLocation = this.PointToScreen(Mouse.GetPosition(this));

            if (this.lastKnownMouseLocation == null)
            {
                this.lastKnownMouseLocation = currentLocation;
            }

            if (this.lastKnownMouseLocation == currentLocation)
            {
                return;
            }

            var diff = currentLocation - this.lastKnownMouseLocation.Value;

            this.TagViewModel.OffsetTop += (int)diff.Y;
            this.TagViewModel.OffsetRight -= (int)diff.X;

            this.lastKnownMouseLocation = currentLocation;
        }

        /// <summary>
        /// The window_ preview mouse up.
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void Window_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.IsMouseCaptured)
            {
                this.ReleaseMouseCapture();
            }
        }
    }
}