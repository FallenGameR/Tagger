using System;
using System.Diagnostics;
using ManagedWinapi.Accessibility;
using Tagger.WinAPI;

namespace Tagger
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Available only in a host process that has at least one Window
    /// </remarks>
    public sealed class WindowMovedListner : IDisposable
    {
        private AccessibleEventListener listner;

        public WindowMovedListner(IntPtr handle)
        {
            int pid = GetPid(handle);

            // Hook via pid
            // TODO: Catch destruction of another window as well
            this.listner = CreateMoveListner(pid);
        }

        public void Dispose()
        {
            this.listner.Dispose();
        }

        /// <summary>
        /// Get actual process ID belonging to host window
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public int GetPid(IntPtr handle)
        {
            int pid;
            NativeAPI.GetWindowThreadProcessId(handle, out pid);

            bool isConsoleApp = this.IsConsoleApplication(pid);
            if (!isConsoleApp)
            {
                return pid;
            }

            using (var wct = new ConhostFinder())
            {
                return wct.GetConhostProcessId(pid);
            }
        }

        /// <summary>
        /// Checks if the application specified by process ID is a console application
        /// </summary>
        /// <param name="pid">Process ID for checked application</param>
        /// <returns>true is application is build as console application</returns>
        private bool IsConsoleApplication(int pid)
        {
            var process = Process.GetProcessById(pid);
            var parser = new PortableExecutableReader(process.MainModule.FileName);
            return parser.OptionalHeader.Subsystem == (ushort)NativeAPI.IMAGE_SUBSYSTEM_WINDOWS.CUI;
        }

        private AccessibleEventListener CreateMoveListner(int pid)
        {
            var listner = new AccessibleEventListener
            {
                MinimalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                MaximalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                ProcessId = (uint)pid,
                Enabled = true,
            };
            listner.EventOccurred += (object sender, AccessibleEventArgs ea) =>
            {
                // Ignore events from cursor
                if (ea.ObjectID != 0) { return; }

                // Invoke event handler
                if (this.Moved != null)
                {
                    this.Moved(this, EventArgs.Empty);
                }
            };
            return listner;
        }

        public event EventHandler Moved;
    }
}
