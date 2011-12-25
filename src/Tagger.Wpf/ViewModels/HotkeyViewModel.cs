using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.Prism;
using Utils.Reflection;
using Tagger.WinAPI.Hotkeys;
using System.Windows.Forms;
using Microsoft.Practices.Prism.Commands;
using System.ComponentModel;
using Utils.Diagnostics;

namespace Tagger.Wpf
{
    /// <summary>
    /// View model for global windows hotkey setting
    /// </summary>
    public class HotkeyViewModel : ViewModelBase
    {
        #region Fields

        private ModifierKeys m_ModifierKeys;
        private Keys m_Key;
        private string m_Status;
        private bool m_IsRegistered;
        private GlobalHotkey m_GlobalHotkey;

        #endregion

        #region Constructors

        public HotkeyViewModel()
        {
            ModifierKeys = ModifierKeys.None;
            Key = Keys.None;
            Status = "No hotkey registered";
            IsRegistered = false;

            RegisterHotkeyCommand = new DelegateCommand<object>(RegisterHotkey, CanRegisterHotkey);
            UnregisterHotkeyCommand = new DelegateCommand<object>(UnregisterHotkey, CanUnregisterHotkey);

            m_GlobalHotkey = new GlobalHotkey();
            m_GlobalHotkey.HotkeyPressed += delegate
            {
                MessageBox.Show("Hotkey pressed.");
            };
        }

        #endregion

        #region IDisposable Members

        protected override void OnDisposeManaged()
        {
            base.OnDisposeManaged();
            m_GlobalHotkey.Dispose();
        }

        #endregion

        #region Command - RegisterHotkey

        /// <summary>
        /// Register global windows hotkey
        /// </summary>
        public DelegateCommand<object> RegisterHotkeyCommand { get; private set; }

        /// <summary>
        /// RegisterHotkey command handler
        /// </summary>
        private void RegisterHotkey(object parameter)
        {
            // Unregister previous hotkey
            if (IsRegistered)
            {
                UnregisterHotkeyCommand.Execute(null);
            }

            // Try to register new global hotkey
            try
            {
                m_GlobalHotkey.RegisterHotKey(ModifierKeys | ModifierKeys.NoRepeat, Key);
            }
            catch (Win32Exception winEx)
            {
                Status = "Failed to register hotkey: " + winEx.Message;
                return;
            }

            // Update dependant fields and commands
            IsRegistered = true;
            Status = "Registered hotkey: " + HotkeyText;
        }

        /// <summary>
        /// Test that verifies if RegisterHotkey command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanRegisterHotkey(object parameter)
        {
            return true;
        }

        #endregion

        #region Command - UnregisterHotkey

        /// <summary>
        /// Unregister previously registered global hotkey
        /// </summary>
        public DelegateCommand<object> UnregisterHotkeyCommand { get; private set; }

        /// <summary>
        /// UnregisterHotkey command handler
        /// </summary>
        private void UnregisterHotkey(object parameter)
        {
            Check.Require(IsRegistered, "You must have a hotkey already registered to unregister its");

            //m_GlobalHotkey.unre

            IsRegistered = false;
        }

        /// <summary>
        /// Test that verifies if UnregisterHotkey command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanUnregisterHotkey(object parameter)
        {
            return IsRegistered;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Modifier keys used for hotkey 
        /// </summary>
        public ModifierKeys ModifierKeys
        {
            get
            {
                return m_ModifierKeys;
            }
            set
            {
                m_ModifierKeys = value;
                OnPropertyChanged(this.Property(() => ModifierKeys));
                OnPropertyChanged(this.Property(() => HotkeyText));
            }
        }

        /// <summary>
        /// Key used in hotkey
        /// </summary>
        public Keys Key
        {
            get
            {
                return m_Key;
            }
            set
            {
                m_Key = value;
                OnPropertyChanged(this.Property(() => Key));
                OnPropertyChanged(this.Property(() => HotkeyText));
            }
        }

        /// <summary>
        /// String representation of hotkey used
        /// </summary>
        public string HotkeyText
        {
            get
            {
                var hotkey = string.Empty;

                if (ModifierKeys.HasFlag(ModifierKeys.Control))
                {
                    hotkey += "Ctrl+";
                }
                if (ModifierKeys.HasFlag(ModifierKeys.Shift))
                {
                    hotkey += "Shift+";
                }
                if (ModifierKeys.HasFlag(ModifierKeys.Alt))
                {
                    hotkey += "Alt+";
                }
                if (ModifierKeys.HasFlag(ModifierKeys.Win))
                {
                    hotkey += "Win+";
                }

                return hotkey + Key.ToString();
            }
        }

        /// <summary>
        /// Hotkey registration status
        /// </summary>
        public string Status
        {
            get { return m_Status; }
            set { m_Status = value; OnPropertyChanged(this.Property(() => Status)); }
        }

        /// <summary>
        /// true if hotkey is registered, false otherwises
        /// </summary>
        public bool IsRegistered
        {
            get
            {
                return m_IsRegistered;
            }
            set
            {
                bool changed = m_IsRegistered != value; 

                m_IsRegistered = value; 
                OnPropertyChanged(this.Property(() => IsRegistered));

                if (changed)
                {
                    OnDelegateCommandsCanExecuteChanged();
                }
            }
        }

        #endregion
    }
}
