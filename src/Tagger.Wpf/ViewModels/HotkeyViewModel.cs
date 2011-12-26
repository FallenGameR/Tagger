using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Tagger.WinAPI.Hotkeys;
using Tagger.Wpf.Properties;
using Utils.Diagnostics;
using Utils.Prism;
using Utils.Reflection;

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
        private GlobalHotkey m_GlobalHotkey;

        #endregion

        #region Constructors

        public HotkeyViewModel()
        {
            // Initialize fields
            m_GlobalHotkey = null;
            RegisterHotkeyCommand = new DelegateCommand<object>(RegisterHotkey, CanRegisterHotkey);
            UnregisterHotkeyCommand = new DelegateCommand<object>(UnregisterHotkey, CanUnregisterHotkey);

            // Restore previous settings state
            ModifierKeys = (ModifierKeys)Settings.Default.Hotkey_Modifiers;
            Key = (Keys)Settings.Default.Hotkey_Keys;

            // Restore hotkey if possible
            if (CanRegisterHotkey(null))
            {
                RegisterHotkeyCommand.Execute(null);
            }
            else
            {
                Status = "No hotkey registered";
            }

            // Save settings on program deactivation (app exit included)
            App.Current.Deactivated += delegate
            {
                Settings.Default.Hotkey_Modifiers = (int)ModifierKeys;
                Settings.Default.Hotkey_Keys = (int)Key;
                Settings.Default.Save();
            };
        }

        #endregion

        #region IDisposable Members

        protected override void OnDisposeManaged()
        {
            base.OnDisposeManaged();

            if (m_GlobalHotkey != null)
            {
                m_GlobalHotkey.Dispose();
                m_GlobalHotkey = null;
            }
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
            Check.Require(CanRegisterHotkey(parameter));

            // Unregister previous hotkey if needed
            if (m_GlobalHotkey != null)
            {
                UnregisterHotkeyCommand.Execute(null);
            }

            // Try to register new global hotkey and update status
            try
            {
                m_GlobalHotkey = new GlobalHotkey(ModifierKeys, Key, (s, a) => HotkeyHandler.HandleHotkeyPress());
                Status = "Registered hotkey: " + HotkeyText;
            }
            catch (Win32Exception winEx)
            {
                Status = "Failed to register hotkey: " + winEx.Message;
            }

            // Update command can execute status
            OnDelegateCommandsCanExecuteChanged();
        }

        /// <summary>
        /// Test that verifies if RegisterHotkey command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanRegisterHotkey(object parameter)
        {
            return Key != Keys.None;
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
            Check.Require(CanUnregisterHotkey(parameter));

            m_GlobalHotkey.Dispose();
            m_GlobalHotkey = null;

            Status = "Hotkey unregistered";
            OnDelegateCommandsCanExecuteChanged();
        }

        /// <summary>
        /// Test that verifies if UnregisterHotkey command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanUnregisterHotkey(object parameter)
        {
            return m_GlobalHotkey != null;
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
                OnDelegateCommandsCanExecuteChanged();
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
                OnDelegateCommandsCanExecuteChanged();
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
                if (ModifierKeys.HasFlag(ModifierKeys.Windows))
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
            private set { m_Status = value; OnPropertyChanged(this.Property(() => Status)); }
        }

        #endregion
    }
}
