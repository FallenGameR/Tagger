using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using ManagedWinapi.Accessibility;
using ManagedWinapi.Windows;
using Tagger.Lib;
using Tagger.WinAPI.WaitChainTraversal;

namespace Tagger.Wpf
{
    public static class HotkeyHandler
    {
        /// <summary>
        /// Retrieves a handle to the foreground window (the window with which the user is currently working)
        /// </summary>
        /// <returns>The return value is a handle to the foreground window. The foreground window can 
        /// be NULL in certain circumstances, such as when a window is losing activation.</returns>
        [DllImport("USER32.DLL")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        
        public static void HandleHotkeyPress()
        {
            var handle = GetForegroundWindow();

            var windowHandle = handle;
            var hostWindow = new SystemWindow(windowHandle);
            var LastKnownPosition = hostWindow.Location.ToString();

            var m_OverlayWindow = new OverlayWindow
            {
                Top = hostWindow.Location.Y,
                Left = hostWindow.Location.X + hostWindow.Size.Width - 200,
                Width = 200,
                Height = 80,
            };
            var wih = new WindowInteropHelper(m_OverlayWindow);
            wih.Owner = hostWindow.HWnd;
            m_OverlayWindow.Show();

            uint pid;
            // TODO: Error handling
            GetWindowThreadProcessId(handle, out pid);


            bool isConsoleApp = LowLevelUtils.IsConsoleApp((int) pid);

            // Get actual process ID belonging to host window
            if (isConsoleApp)
            {
                using (var wct = new ProcessFinder())
                {
                    pid = (uint) wct.GetConhostProcess((int)pid);
                }
            }


            // Hook pid (available only in Windowed process)
            var m_Listner = new AccessibleEventListener
            {
                MinimalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                MaximalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                ProcessId = (uint)pid,
                Enabled = true,
            };
            m_Listner.EventOccurred += (object sender, AccessibleEventArgs e) =>
            {
                // Ignore events from cursor
                if (e.ObjectID != 0) { return; }

                LastKnownPosition = e.AccessibleObject.Location.ToString();

                m_OverlayWindow.Top = e.AccessibleObject.Location.Top;
                m_OverlayWindow.Left = e.AccessibleObject.Location.X + e.AccessibleObject.Location.Width - 200;
            };



            //MessageBox.Show("Current window: " + GetActiveWindowTitle());

        }

    }
}
