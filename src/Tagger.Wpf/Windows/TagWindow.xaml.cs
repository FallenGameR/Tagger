using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using ManagedWinapi.Accessibility;
using Tagger.WinAPI;
using Tagger.WinAPI.WaitChainTraversal;
using Utils.Diagnostics;
using RECT = Tagger.WinAPI.NativeAPI.RECT;

namespace Tagger.Wpf
{
    /// <summary>
    /// Interaction logic for OverlayWindow.xaml
    /// </summary>
    public partial class TagWindow : Window
    {
        public TagWindow()
        {
            InitializeComponent();
        }

        public TagWindow(IntPtr handle)
            : this()
        {
            this.HostHandle = handle;
            this.HookToForegroundWindow();
            this.Show();
        }

        // TODO: Delete on dispose.
        private AccessibleEventListener m_Listner;

        /// <summary>
        /// Native handle corresponding to this window
        /// </summary>
        public IntPtr Handle
        {
            get { return new WindowInteropHelper(this).Handle; }
        }

        public IntPtr HostHandle { get; set; }

        public void HookToForegroundWindow()
        {
            Check.Require(m_Listner == null, "Location change listner should not have been initialized");

            new WindowInteropHelper(this).Owner = HostHandle;

            UpdateLocation(HostHandle);

            uint pid = GetPid(HostHandle);

            // Hook pid (available only in Windowed process)
            // TODO: Catch destruction of another window as well
            // TODO: Dispose on exit
            m_Listner = new AccessibleEventListener
            {
                MinimalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                MaximalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                ProcessId = pid,
                Enabled = true,
            };
            m_Listner.EventOccurred += (object sender, AccessibleEventArgs e) =>
            {
                // Ignore events from cursor
                if (e.ObjectID != 0) { return; }

                UpdateLocation(HostHandle);
            };
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
            Top = clientArea.Top;
            Left = clientArea.Right - Width;
        }

        private static uint GetPid(IntPtr handle)
        {
            // TODO: Move to domain logic
            uint pid;
            NativeAPI.GetWindowThreadProcessId(handle, out pid);

            // Get actual process ID belonging to host window
            bool isConsoleApp = ConsoleDeterminer.IsConsoleApplication((int)pid);
            if (isConsoleApp)
            {
                using (var wct = new ProcessFinder())
                {
                    pid = (uint)wct.GetConhostProcess((int)pid);
                }
            }

            return pid;
        }

        private void Window_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // TODO: Show settings dialog
        }
    }
}
