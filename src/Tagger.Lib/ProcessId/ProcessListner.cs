using System;
using System.Diagnostics;
using System.Windows.Automation;
using ManagedWinapi.Accessibility;
using Tagger.WinAPI;
using Utils.Diagnostics;

namespace Tagger
{
    /// <summary>
    /// Listner of events happening in a window that belongs to another process
    /// </summary>
    /// <remarks>
    /// Would work only when invoked from a process that has at least one Window.
    /// SetWindowHook WinAPI function that is used under the hood need window
    /// message loop to perform cross process communication.
    /// 
    /// For tracking window destruction UIAutomation is used instead of hooking 
    /// EVENT_OBJECT_DESTROY since hook needs additional filtering to be done.
    /// See Raymond Chen's "How can I get notified when some other window is destroyed?"
    /// for details - http://blogs.msdn.com/b/oldnewthing/archive/2011/10/26/10230020.aspx
    /// </remarks>
    public sealed class ProcessListner : IDisposable
    {
        private AccessibleEventListener moveListner;
        private AutomationElement automationWindowElement;
        private AutomationEventHandler automationWindowCloseEventHandler;

        /// <summary>
        /// Initializes WindowMovedListner instance
        /// </summary>
        /// <param name="windowHandle">Window that we need to track</param>
        public ProcessListner(IntPtr windowHandle)
        {
            // Listen for destruction event
            this.automationWindowElement = AutomationElement.FromHandle(windowHandle);
            this.automationWindowCloseEventHandler = new AutomationEventHandler(OnWindowCloseHandler);
            Automation.AddAutomationEventHandler(
                WindowPattern.WindowClosedEvent, 
                this.automationWindowElement, 
                TreeScope.Element, 
                this.automationWindowCloseEventHandler);
            
            // Listen for move events
            this.moveListner = new AccessibleEventListener
            {
                MinimalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                MaximalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                ProcessId = (uint)ProcessListner.GetPidFromWindow(windowHandle),
                Enabled = true,
            };
            this.moveListner.EventOccurred += this.OnWindowMoveHandler;
        }

        /// <summary>
        /// Cleanup and dispose all listners
        /// </summary>
        public void Dispose()
        {
            this.moveListner.EventOccurred -= this.OnWindowMoveHandler;
            this.moveListner.Dispose();

            Automation.RemoveAutomationEventHandler(
                WindowPattern.WindowClosedEvent,
                this.automationWindowElement,
                this.automationWindowCloseEventHandler);
        }

        /// <summary>
        /// Handler for window close event
        /// </summary>
        /// <param name="sender">Object that raised the event</param>
        /// <param name="ea">Event arguments</param>
        private void OnWindowCloseHandler(object sender, AutomationEventArgs ea)
        {
            Check.Ensure(ea.EventId == WindowPattern.WindowClosedEvent, "We subscribed only to the closed event");

            // Invoke event handler
            if (this.Destroyed != null)
            {
                this.Destroyed(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handler for window move event
        /// </summary>
        /// <param name="sender">Object that raised the event</param>
        /// <param name="ea">Event arguments</param>
        private void OnWindowMoveHandler(object sender, AccessibleEventArgs ea)
        {
            // Ignore events from cursor
            if (ea.ObjectID != 0) { return; }

            // Invoke event handler
            if (this.Moved != null)
            {
                this.Moved(this, EventArgs.Empty);
            }
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
        private static int GetPidFromWindow(IntPtr windowHandle)
        {
            int pid;
            NativeAPI.GetWindowThreadProcessId(windowHandle, out pid);

            bool isConsoleApp = ProcessListner.IsConsoleApplication(pid);
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
        private static bool IsConsoleApplication(int pid)
        {
            var process = Process.GetProcessById(pid);
            var parser = new PortableExecutableReader(process.MainModule.FileName);
            return parser.OptionalHeader.Subsystem == (ushort)NativeAPI.IMAGE_SUBSYSTEM_WINDOWS.CUI;
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
        /// 
        /// Couldn't find how the same could be done via UI Automation.
        /// </remarks>
        public event EventHandler Moved;

        /// <summary>
        /// Event that fires whenever host window is destroyed
        /// </summary>
        /// <remarks>
        /// Implemented via UI Automation since there is build-in filtering for 
        /// event that gives event relevent for the window destruction. See class
        /// remarks for more info.
        /// </remarks>
        public event EventHandler Destroyed;
    }
}
