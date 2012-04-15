using System;
using System.Windows;
using System.Windows.Input;

namespace Tagger.Wpf
{
    public sealed partial class TagOverlayWindow : Window, IDisposable
    {
        private Point? m_lastLocation;

        public TagOverlayWindow()
        {
            InitializeComponent();

            this.PreviewMouseMove += Window_PreviewMouseMove;
            this.PreviewMouseUp += Window_PreviewMouseUp;
            this.LocationChanged += Window_LocationChanged;
            this.PreviewMouseDown += Window_PreviewMouseDown;
        }

        public void Dispose()
        {
            this.PreviewMouseMove -= Window_PreviewMouseMove;
            this.PreviewMouseUp -= Window_PreviewMouseUp;
            this.LocationChanged -= Window_LocationChanged;
            this.PreviewMouseDown -= Window_PreviewMouseDown;
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            m_lastLocation = this.PointToScreen(Mouse.GetPosition(this));
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            m_lastLocation = this.PointToScreen(Mouse.GetPosition(this));
        }

        private void Window_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.IsMouseCaptured)
            {
                this.ReleaseMouseCapture();
            }
        }

        /// <summary>
        /// Drag tag via offset property modification
        /// </summary>
        /// <remarks>
        /// Original code from http://groups.google.com/group/wpf-disciples/browse_thread/thread/ece72e5eb7f6c217?pli=1
        /// </remarks>
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

            if (m_lastLocation == null)
            {
                m_lastLocation = currentLocation;
            }

            if (m_lastLocation == currentLocation)
            {
                return;
            }

            var diff = currentLocation - m_lastLocation.Value;
            
            this.TagViewModel.OffsetTop += (int)diff.Y;
            this.TagViewModel.OffsetRight -= (int)diff.X;

            m_lastLocation = currentLocation;
        }

        private TagViewModel TagViewModel { get { return (TagViewModel)this.DataContext; } }
    }
}
