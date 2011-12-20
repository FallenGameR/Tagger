﻿using System.Diagnostics;
using System.Linq;
using ManagedWinapi.Accessibility;
using ManagedWinapi.Windows;
using Microsoft.Practices.Prism.Commands;
using Tagger.Lib;
using Tagger.WinAPI.WaitChainTraversal;
using Utils.Prism;
using Utils.Reflection;

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

        #region Validation

        /// <summary>
        /// Validation for ProcessId property value
        /// </summary>
        private void Validate_ProcessId()
        {
            Validate(m_ProcessId > 0, "Process ID must be greater than 0");
        }

        #endregion

        private void ResetPropertyValues()
        {
            ProcessId = -1;
            ApplicationType = "Not set";
            IsHooked = false;
            LastKnownPosition = "Not known";
        }

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
        /// Hook command handler
        /// </summary>
        private void Hook(object parameter)
        {
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
            LastKnownPosition = new SystemWindow(windowHandle).Location.ToString();

            // Hook pid (available only in Windowed process)
            UnhookCommand.Execute(null);
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
