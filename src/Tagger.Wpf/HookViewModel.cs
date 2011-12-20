using System.Diagnostics;
using System.Linq;
using ManagedWinapi.Accessibility;
using ManagedWinapi.Windows;
using Microsoft.Practices.Prism.Commands;
using Tagger.Lib;
using Tagger.WinAPI.WaitChainTraversal;
using Utils.Prism;
using Utils.Reflection;
using System.Runtime.InteropServices;
using System;
using System.Windows.Interop;
using ManagedWinapi.Hooks;

namespace Tagger.Wpf
{
    /// <summary>
    /// View model for control that allows to hook window
    /// movement events comming from another process
    /// </summary>
    public class HookViewModel : ViewModelBase
    {
        #region Fields

        private int m_ProcessId;
        private string m_ApplicationType;
        private bool m_IsHooked;
        private string m_LastKnownPosition;
        private AccessibleEventListener m_Listner;
        private OverlayWindow m_OverlayWindow;

        #endregion

        #region Constructor

        public HookViewModel()
        {
            ResetPropertyValues();

            StartConsoleApplicationCommand = new DelegateCommand<object>(StartConsoleApplication, CanStartConsoleApplication);
            StartWindowedApplicationCommand = new DelegateCommand<object>(StartWindowedApplication, CanStartWindowedApplication);
            HookCommand = new DelegateCommand<object>(Hook, CanHook);
            UnhookCommand = new DelegateCommand<object>(Unhook, CanUnhook);
        }

        #endregion

        #region IDisposable Members

        protected override void OnDisposeManaged()
        {
            base.OnDisposeManaged();
            UnhookCommand.Execute(null);
        }

        #endregion

        #region Methods

        private void ResetPropertyValues()
        {
            ProcessId = -1;
            ApplicationType = "Not set";
            IsHooked = false;
            LastKnownPosition = "Not known";
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validation for ProcessId property value
        /// </summary>
        private void Validate_ProcessId()
        {
            Validate(m_ProcessId > 0, "Process ID must be greater than 0");
        }

        #endregion

        #region Command - StartConsoleApplication

        /// <summary>
        /// Starts a console application
        /// </summary>
        public DelegateCommand<object> StartConsoleApplicationCommand { get; private set; }

        /// <summary>
        /// StartConsoleApplication command handler
        /// </summary>
        private void StartConsoleApplication(object parameter)
        {
            ProcessId = Process.Start("powershell.exe").Id;


            //var hook = new LowLevelKeyboardHook();
            //hook.CharIntercepted += new LowLevelKeyboardHook.CharCallback(hook_CharIntercepted);
            //hook.KeyIntercepted += new LowLevelKeyboardHook.KeyCallback(hook_KeyIntercepted);
            //hook.MessageIntercepted += new LowLevelMessageCallback(hook_MessageIntercepted);
            //hook.StartHook();
        }

        void hook_MessageIntercepted(LowLevelMessage evt, ref bool handled)
        {
            this.LastKnownPosition = "msg: " + evt.Message.ToString();
        }

        void hook_KeyIntercepted(int msg, int vkCode, int scanCode, int flags, int time, IntPtr dwExtraInfo, ref bool handled)
        {
            this.LastKnownPosition = "key: " + scanCode.ToString() + " " + vkCode.ToString();
        }

        void hook_CharIntercepted(int msg, string characters, bool deadKeyPending, int vkCode, int scancode, int flags, int time, IntPtr dwExtraInfo)
        {
            this.LastKnownPosition = "char: " + characters;
        }

        /// <summary>
        /// Test that verifies if StartConsoleApplication command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanStartConsoleApplication(object parameter)
        {
            return true;
        }

        #endregion

        #region Command - StartWindowedApplication

        /// <summary>
        /// Start a windowed application
        /// </summary>
        public DelegateCommand<object> StartWindowedApplicationCommand { get; private set; }

        /// <summary>
        /// StartWindowedApplication command handler
        /// </summary>
        private void StartWindowedApplication(object parameter)
        {
            ProcessId = Process.Start("calc.exe").Id;
        }

        /// <summary>
        /// Test that verifies if StartWindowedApplication command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanStartWindowedApplication(object parameter)
        {
            return true;
        }

        #endregion

        #region Command - Hook

        /// <summary>
        /// Command that hooks some process
        /// </summary>
        public DelegateCommand<object> HookCommand { get; private set; }

        /// <summary>
        /// Changes an attribute of the specified window. The function also sets the 32-bit (long) value at the specified offset into the extra window memory.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs..</param>
        /// <param name="nIndex">The zero-based offset to the value to be set. Valid values are in the range zero through the number of bytes of extra window memory, minus the size of an integer. To set any other value, specify one of the following values: GWL_EXSTYLE, GWL_HINSTANCE, GWL_ID, GWL_STYLE, GWL_USERDATA, GWL_WNDPROC </param>
        /// <param name="dwNewLong">The replacement value.</param>
        /// <returns>If the function succeeds, the return value is the previous value of the specified 32-bit integer.
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError. </returns>
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        /// <summary>
        /// Hook command handler
        /// </summary>
        private void Hook(object parameter)
        {
            UnhookCommand.Execute(null);

            // Test if it is a console app
            bool isConsoleApp = LowLevelUtils.IsConsoleApp(ProcessId);
            if (isConsoleApp)
            {
                ApplicationType = "Console";
            }
            else
            {
                ApplicationType = "GUI";
            }

            // Get actual process ID belonging to host window
            int pid = ProcessId;
            if (isConsoleApp)
            {
                using (var wct = new ProcessFinder())
                {
                    pid = wct.GetConhostProcess(pid);
                }
            }

            // Get current window position
            var windowHandle = Process.GetProcessById(ProcessId).MainWindowHandle;
            var hostWindow = new SystemWindow(windowHandle);
            LastKnownPosition = hostWindow.Location.ToString();

            m_OverlayWindow = new OverlayWindow
            {
                Top = hostWindow.Location.Y,
                Left = hostWindow.Location.X + hostWindow.Size.Width - 200,
                Width = 200,
                Height = 80,
            };
            var wih = new WindowInteropHelper(m_OverlayWindow);
            wih.Owner = hostWindow.HWnd;
            m_OverlayWindow.Show();

            // Hook pid (available only in Windowed process)
            m_Listner = new AccessibleEventListener
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

            // Mark window as hooked and recalculate all comands
            IsHooked = true;
            OnDelegateCommandsCanExecuteChanged();
        }

        /// <summary>
        /// Test that verifies if Hook command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanHook(object parameter)
        {
            return !Errors.Any() && !IsHooked;
        }

        #endregion

        #region Command - Unhook

        /// <summary>
        /// Coomands that unhooks a previously set hook
        /// </summary>
        public DelegateCommand<object> UnhookCommand { get; private set; }

        /// <summary>
        /// Unhook command handler
        /// </summary>
        private void Unhook(object parameter)
        {
            if (m_Listner != null)
            {
                m_Listner.Dispose();
                m_Listner = null;
                ResetPropertyValues();
                OnDelegateCommandsCanExecuteChanged();
            }

            if (m_OverlayWindow != null)
            {
                m_OverlayWindow.Close();
                m_OverlayWindow = null;
            }
        }

        /// <summary>
        /// Test that verifies if Unhook command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanUnhook(object parameter)
        {
            return !Errors.Any() && IsHooked;
        }

        #endregion

        #region Properties

        /// <summary>
        /// ID of the process that is being hooked
        /// </summary>
        public int ProcessId
        {
            get
            {
                return m_ProcessId;
            }
            set
            {
                if (!IsHooked)
                {
                    m_ProcessId = value;
                }
                OnPropertyChanged(this.Property(() => ProcessId));
            }
        }

        /// <summary>
        /// Type of the application - is it Console or GUI application
        /// </summary>
        public string ApplicationType
        {
            get { return m_ApplicationType; }
            private set { m_ApplicationType = value; OnPropertyChanged(this.Property(() => ApplicationType)); }
        }

        /// <summary>
        /// True if the hook was applied on the application
        /// </summary>
        public bool IsHooked
        {
            get { return m_IsHooked; }
            private set { m_IsHooked = value; OnPropertyChanged(this.Property(() => IsHooked)); }
        }

        /// <summary>
        /// Last known position of application window
        /// </summary>
        public string LastKnownPosition
        {
            get { return m_LastKnownPosition; }
            private set { m_LastKnownPosition = value; OnPropertyChanged(this.Property(() => LastKnownPosition)); }
        }

        #endregion
    }
}
