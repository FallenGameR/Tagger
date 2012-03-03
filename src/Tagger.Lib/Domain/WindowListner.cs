using System;
using System.Diagnostics;
using System.Windows.Automation;
using System.Windows.Threading;
using ManagedWinapi.Accessibility;
using Tagger.WinAPI;
using Utils.Diagnostics;
using Utils.Extensions;

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
    /// 
    /// On dispose unregisters all handlers from all its events. That's an unusual 
    /// behaviour but it allows to garbage collect tag context without additional clutter.
    /// </remarks>
    public sealed class WindowListner : IDisposable
    {
        private IntPtr windowHandle;
        private AccessibleEventListener moveListner;
        private AutomationElement automationWindowElement;
        private AutomationEventHandler automationWindowCloseEventHandler;
        private DispatcherTimer fallbackStrategyTimer;

        /// <summary>
        /// Initializes WindowMovedListner instance
        /// </summary>
        /// <param name="windowHandle">Window that we need to track</param>
        public WindowListner(IntPtr windowHandle)
        {
            Check.Require(windowHandle != IntPtr.Zero, "Window handle must not be zero");
            this.windowHandle = windowHandle;

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
                ProcessId = (uint)WindowListner.GetPidFromWindow(windowHandle),
                Enabled = true,
            };
            this.moveListner.EventOccurred += this.OnWindowMoveHandler;

            // Use fallback strategy that manually redraws tag position once in 1/3 of a second
            this.fallbackStrategyTimer = new DispatcherTimer();
            this.fallbackStrategyTimer.Interval = TimeSpan.FromMilliseconds(300);
            this.fallbackStrategyTimer.Tick += this.FallbackStrategyHandler;
            this.fallbackStrategyTimer.Start();
        }

        /// <summary>
        /// Cleanup and dispose all listners
        /// </summary>
        public void Dispose()
        {
            // All clients are forecefully unsubsribed so that garbage collection would succeed 
            // even if we used lambda expressions and anonymous delegates as event handlers.
            // There isn't any scence to preserve subscription since we dispose original event 
            // sources latter in this method - there would not be any new events anyway.
            this.ClientAreaChanged.GetInvocationList().Action(d => this.ClientAreaChanged -= (EventHandler) d);
            this.WindowDestroyed.GetInvocationList().Action(d => this.WindowDestroyed -= (EventHandler)d);

            // Stop falback strategy timer
            this.fallbackStrategyTimer.Stop();

            // Cleanup window move listner
            this.moveListner.EventOccurred -= this.OnWindowMoveHandler;
            this.moveListner.Dispose();

            // Cleanup window destroy listner
            Automation.RemoveAutomationEventHandler(
                WindowPattern.WindowClosedEvent,
                this.automationWindowElement,
                this.automationWindowCloseEventHandler);
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

            // Stop fallback strategy timer if enabled
            if (this.fallbackStrategyTimer.IsEnabled)
            {
                this.fallbackStrategyTimer.Stop();
            }

            // Invoke event handler
            if (this.ClientAreaChanged != null)
            {
                this.ClientAreaChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Fallback strategy handler
        /// </summary>
        /// <remarks>
        /// There are cases where SetWindowsHook would silently fail. It would return a valid
        /// handle but would never report any move error. Repro is - try to set up a tag on 
        /// a console window that started to execute very long IO intensive task (like copying
        /// large folder). For such cases fallback strategy is used - manually redraw tag position
        /// each 1/3 of a second. If there would be at least one event from SetWindowsHook, the 
        /// fallback strategy is suppressed and never used again for this tag to preserve resources.
        /// </remarks>
        private void FallbackStrategyHandler(object sender, EventArgs e)
        {
            // Invoke event handler
            if (this.ClientAreaChanged != null)
            {
                this.ClientAreaChanged(this, EventArgs.Empty);
            }
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
            if (this.WindowDestroyed != null)
            {
                this.WindowDestroyed(this, EventArgs.Empty);
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

            bool isConsoleApp = WindowListner.IsConsoleApplication(pid);
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
        /// Event that fires whenever host window client area is changed
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
        public event EventHandler ClientAreaChanged;

        /// <summary>
        /// Event that fires whenever host window is destroyed
        /// </summary>
        /// <remarks>
        /// Implemented via UI Automation since there is build-in filtering for 
        /// event that gives event relevent for the window destruction. See class
        /// remarks for more info.
        /// </remarks>
        public event EventHandler WindowDestroyed;
    }
}
