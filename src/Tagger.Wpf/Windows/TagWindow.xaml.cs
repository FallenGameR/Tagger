using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using ManagedWinapi.Accessibility;
using Tagger;
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
        // TODO: Delete on dispose.
        private WindowMovedListner m_Listner;

        public TagWindow()
        {
            InitializeComponent();
        }

        public TagWindow(IntPtr handle)
            : this()
        {
            new WindowInteropHelper(this).Owner = handle;
            UpdateLocation(handle);

            m_Listner = new WindowMovedListner(handle);
            m_Listner.Moved += delegate { UpdateLocation(handle); };

            this.Show();
        }

        public void Dispose()
        {
            m_Listner.Dispose();
        }

        /// <summary>
        /// Native handle corresponding to this window
        /// </summary>
        public IntPtr Handle
        {
            get { return new WindowInteropHelper(this).Handle; }
        }

        private static RECT GetClientArea(IntPtr handle)
        {
            RECT sizes;
            bool success = NativeAPI.GetWindowRect(handle, out sizes);

            if (!success)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            var zero = NativeAPI.SendMessage(handle, NativeAPI.WM_NCCALCSIZE, 0, ref sizes);
            Check.Ensure(zero == 0);

            return sizes;
        }

        private void UpdateLocation(IntPtr handle)
        {
            RECT clientArea = GetClientArea(handle);
            this.Top = clientArea.Top;
            this.Left = clientArea.Right - Width;
        }

        private void Window_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //this.MouseRightButtonUp
            // TODO: Show settings dialog
        }
    }
}
