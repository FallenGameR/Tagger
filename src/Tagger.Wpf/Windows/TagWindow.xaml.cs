using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Tagger.WinAPI;
using Tagger.Wpf.ViewModels;
using Utils.Diagnostics;
using RECT = Tagger.WinAPI.NativeAPI.RECT;

namespace Tagger.Wpf
{
    /// <summary>
    /// Interaction logic for OverlayWindow.xaml
    /// </summary>
    public sealed partial class TagWindow : Window
    {
        /// <summary>
        /// Listner that fires events on window moves
        /// </summary>
        private WindowMovedListner windowMovedListner;

        /// <summary>
        /// Constructor that is used by the studio designer
        /// </summary>
        public TagWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes and shows new instance of tag window 
        /// </summary>
        /// <param name="host">Host window this tag belong to</param>
        /// <param name="viewModel">View model with all settings needed for tag render</param>
        public TagWindow(IntPtr host, TagModel viewModel)
            : this()
        {
            // Bind to view model
            this.DataContext = viewModel;

            // Set window owner so that the tag would always be on top of it
            this.Host = host;

            // Subscribe to the tagged window movements
            this.windowMovedListner = new WindowMovedListner(host);
            this.windowMovedListner.Moved += delegate { this.UpdateTagPosition(); };

            // Show tag window in the right position
            this.Show();
            this.UpdateTagPosition();
        }

        /// <summary>
        /// Cleanup all allocated resources
        /// </summary>
        public void Dispose()
        {
            if (this.windowMovedListner != null)
            {
                this.windowMovedListner.Dispose();
            }
        }

        /// <summary>
        /// Update tag position based on host window position
        /// </summary>
        private void UpdateTagPosition()
        {
            RECT clientArea = this.GetHostClientArea();

            this.Top = clientArea.Top;
            this.Left = clientArea.Right - Width;
        }

        /// <summary>
        /// Gets host client area rectangle
        /// </summary>
        /// <returns>
        /// Rectangle that borders host window content
        /// </returns>
        private RECT GetHostClientArea()
        {
            RECT sizes;
            bool success = NativeAPI.GetWindowRect(this.Host, out sizes);

            if (!success)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            var zero = NativeAPI.SendMessage(this.Host, NativeAPI.WM_NCCALCSIZE, 0, ref sizes);
            Check.Ensure(zero == 0);

            return sizes;
        }

        /// <summary>
        /// TODO: Show settings dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// Native handle corresponding to this tag window
        /// </summary>
        public IntPtr Handle
        {
            get { return new WindowInteropHelper(this).Handle; }
        }

        /// <summary>
        /// Native handle for host window (the owner window this tag belongs to)
        /// </summary>
        private IntPtr Host
        {
            get { return new WindowInteropHelper(this).Owner; }
            set { new WindowInteropHelper(this).Owner = value; }
        }
    }
}
