﻿using System;
using System.Diagnostics;
using ManagedWinapi.Accessibility;
using Tagger.WinAPI;

namespace Tagger
{
    /// <summary>
    /// Listner of another events happening in a window that belongs to another process
    /// </summary>
    /// <remarks>
    /// Would work only when invoked from a process that has at least one Window.
    /// SetWindowHook WinAPI function that is used under the hood need window
    /// message loop to perform cross process communication.
    /// </remarks>
    public sealed class WindowMovedListner : IDisposable
    {
        private AccessibleEventListener moveListner;

        // TODO: Also listen: destruction of another window, minimize/maximize

        /// <summary>
        /// Initializes WindowMovedListner instance
        /// </summary>
        /// <param name="windowHandle">Window that we need to track</param>
        public WindowMovedListner(IntPtr windowHandle)
        {
            int pid = this.GetPidFromWindow(windowHandle);
            this.moveListner = this.CreateMoveListner(pid);
        }

        /// <summary>
        /// Cleanup all disposable resources
        /// </summary>
        public void Dispose()
        {
            this.moveListner.Dispose();
        }

        /// <summary>
        /// Get process ID that corresponds to the window
        /// </summary>
        /// <param name="windowHandle">Window that we need to track</param>
        /// <returns>
        /// PID of process that owns the window (GUI applications)
        /// PID of conhost.exe process that renders console application (CUI applications)
        /// </returns>
        /// <remarks>
        /// CUI application support is only for Windows 7, Windows Server 2008 R2.
        /// Previous versions of Windows didn't have conhost.exe process to host CUI app.
        /// </remarks>
        private int GetPidFromWindow(IntPtr windowHandle)
        {
            int pid;
            NativeAPI.GetWindowThreadProcessId(windowHandle, out pid);

            bool isConsoleApp = this.IsConsoleApplication(pid);
            if (!isConsoleApp)
            {
                return pid;
            }

            using (var finder = new ConhostFinder())
            {
                return finder.GetConhostProcessId(pid);
            }
        }

        /// <summary>
        /// Checks if the application specified by process ID is a console application
        /// </summary>
        /// <param name="pid">Process ID for the checked application</param>
        /// <returns>true is application is build as console application</returns>
        private bool IsConsoleApplication(int pid)
        {
            var process = Process.GetProcessById(pid);
            var parser = new PortableExecutableReader(process.MainModule.FileName);
            return parser.OptionalHeader.Subsystem == (ushort)NativeAPI.IMAGE_SUBSYSTEM_WINDOWS.CUI;
        }

        /// <summary>
        /// Create listner that would listen to move events from windows of specified process
        /// </summary>
        /// <param name="pid">Process ID that owns tracked windows</param>
        /// <returns>Listner object to handle lifetime</returns>
        private AccessibleEventListener CreateMoveListner(int pid)
        {
            // Create listner for move event of the specified process
            var listner = new AccessibleEventListener
            {
                MinimalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                MaximalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                ProcessId = (uint)pid,
                Enabled = true,
            };

            // Setup event handler
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

        /// <summary>
        /// Event that fires whenever host window is moved or resized
        /// </summary>
        /// <remarks>
        /// Event fires in the thread it was regestered from. Under the hood there is 
        /// multithreading and cross process communication, but all of this is hidden 
        /// via SetWindowsHook WinAPI function and thin AccessibleEventType wrapper.
        /// 
        /// To see C++ implementation go back in project history - 
        /// there used to be a PoC project for that.
        /// </remarks>
        public event EventHandler Moved;
    }
}
