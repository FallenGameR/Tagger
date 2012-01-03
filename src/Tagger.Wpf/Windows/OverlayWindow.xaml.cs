using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using ManagedWinapi.Accessibility;
using Tagger.Lib;
using Tagger.WinAPI.WaitChainTraversal;
using Utils.Diagnostics;

namespace Tagger.Wpf
{
    /// <summary>
    /// Interaction logic for OverlayWindow.xaml
    /// </summary>
    public partial class OverlayWindow : Window
    {
        public OverlayWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Retrieves a handle to the foreground window (the window with which the user is currently working)
        /// </summary>
        /// <returns>The return value is a handle to the foreground window. The foreground window can 
        /// be NULL in certain circumstances, such as when a window is losing activation.</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// Retrieves the identifier of the thread that created the specified window and, 
        /// optionally, the identifier of the process that created the window. 
        /// </summary>
        /// <param name="hWnd">A handle to the window.</param>
        /// <param name="lpdwProcessId">A pointer to a variable that receives the process identifier.</param>
        /// <returns>The return value is the identifier of the thread that created the window.</returns>
        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        // TODO: Delete on dispose.
        private AccessibleEventListener m_Listner;

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, UInt32 msg, Int32 wParam, ref RECT lParam);

        /// <summary>
        /// The WM_NCCALCSIZE message is sent when the size and position of a window's client area 
        /// must be calculated. By processing this message, an application can control the content 
        /// of the window's client area when the size or position of the window changes.
        /// </summary>
        static uint WM_NCCALCSIZE = 0x83;

        public void HookToForegroundWindow()
        {
            Check.Require(m_Listner == null, "Location change listner should not have been initialized");

            var handle = GetForegroundWindow();
            new WindowInteropHelper(this).Owner = handle;

            UpdateLocation(handle);

            uint pid = GetPid(handle);

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

                UpdateLocation(handle);
            };
        }

        private static RECT GetClientArea(IntPtr handle)
        {
            RECT sizes;
            bool success = GetWindowRect(handle, out sizes);

            if (!success)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            var zero = SendMessage(handle, WM_NCCALCSIZE, 0, ref sizes);
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
            GetWindowThreadProcessId(handle, out pid);

            // Get actual process ID belonging to host window
            bool isConsoleApp = LowLevelUtils.IsConsoleApp((int)pid);
            if (isConsoleApp)
            {
                using (var wct = new ProcessFinder())
                {
                    pid = (uint)wct.GetConhostProcess((int)pid);
                }
            }

            return pid;
        }
    }
}
