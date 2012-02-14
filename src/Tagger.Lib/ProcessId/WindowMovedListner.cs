using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManagedWinapi.Accessibility;
using Tagger.WinAPI;
using System.Diagnostics;

namespace Tagger
{
    public sealed class WindowMovedListner: IDisposable
    {
        private AccessibleEventListener m_Listner;

        public WindowMovedListner(IntPtr handle)
        {
            uint pid = ConhostFinder.GetPid(handle);

            // Hook via pid
            // NOTE: available only in a host process that has at least one Window
            // TODO: Catch destruction of another window as well
            m_Listner = new AccessibleEventListener
            {
                MinimalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                MaximalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                ProcessId = pid,
                Enabled = true,
            };
            m_Listner.EventOccurred += (object sender, AccessibleEventArgs ea) =>
            {
                // Ignore events from cursor
                if (ea.ObjectID != 0) { return; }

                // Invoke event handler
                if (this.Moved != null)
                {
                    this.Moved(this, EventArgs.Empty);
                }
            };
        }

        public static uint GetPid(IntPtr handle)
        {
            // TODO: Move to domain logic
            uint pid;
            NativeAPI.GetWindowThreadProcessId(handle, out pid);

            // Get actual process ID belonging to host window
            bool isConsoleApp = WindowMovedListner.IsConsoleApplication((int)pid);
            if (isConsoleApp)
            {
                using (var wct = new ConhostFinder())
                {
                    pid = (uint)wct.GetConhostProcessId((int)pid);
                }
            }

            return pid;
        }

        /// <summary>
        /// Checks if the application specified by process ID is a console application
        /// </summary>
        /// <param name="pid">Process ID for checked application</param>
        /// <returns>true is application is build as console application</returns>
        private static bool IsConsoleApplication(int pid)
        {
            var process = Process.GetProcessById(pid);
            var parser = new PortableExecutableReader(process.MainModule.FileName);
            return parser.OptionalHeader.Subsystem == (ushort)NativeAPI.IMAGE_SUBSYSTEM_WINDOWS.CUI;
        }


        public void Dispose()
        {
            m_Listner.Dispose();
        }
        
        public event EventHandler Moved;
    }
}
