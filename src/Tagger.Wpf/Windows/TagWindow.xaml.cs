using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Tagger.WinAPI;
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
        /// Initializes tag window 
        /// </summary>
        /// <param name="handle"></param>
        public TagWindow(IntPtr handle)
            : this()
        {
            // Set window owner so that our window would always be on top
            new WindowInteropHelper(this).Owner = handle;

            // Subscribe to the tagged window movements
            this.windowMovedListner = new WindowMovedListner(handle);
            this.windowMovedListner.Moved += delegate { this.UpdateLocation(); };
        }

        public void Dispose()
        {
            if (this.windowMovedListner != null)
            {
                this.windowMovedListner.Dispose();
            }
        }

        public void UpdateLocation()
        {
            RECT clientArea = this.GetClientArea();

            this.Top = clientArea.Top;
            this.Left = clientArea.Right - Width;
        }

        private RECT GetClientArea()
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

        private void Window_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //this.MouseRightButtonUp
            // TODO: Show settings dialog
        }

        /// <summary>
        /// Native handle corresponding to this window
        /// </summary>
        public IntPtr Handle
        {
            get { return new WindowInteropHelper(this).Handle; }
        }

        public IntPtr Host
        {
            get { return new WindowInteropHelper(this).Owner; }
        }

    }
}
