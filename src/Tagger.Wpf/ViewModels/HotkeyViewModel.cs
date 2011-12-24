using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.Prism;
using Utils.Reflection;
using Tagger.WinAPI.Hotkeys;
using System.Windows.Forms;
using Microsoft.Practices.Prism.Commands;

namespace Tagger.Wpf
{
    /// <summary>
    /// View model for global windows hotkey setting
    /// </summary>
    public class HotkeyViewModel : ViewModelBase
    {
        //globalHotkey = new GlobalHotkey();
        //globalHotkey.HotkeyPressed += delegate
        //{
        //    MessageBox.Show("Hotkey pressed.");
        //};
        //globalHotkey.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.NoRepeat, Keys.Z);

        #region Fields

        private ModifierKeys m_ModifierKeys;
        private Keys m_Key;
        private string m_Status;

        #endregion

        #region Constructors

        public HotkeyViewModel()
        {
            ModifierKeys = ModifierKeys.None;
            Key = Keys.None;
            Status = "No hotkey registered";

            RegisterHotkeyCommand = new DelegateCommand<object>(RegisterHotkey, CanRegisterHotkey);
            UnregisterHotkeyCommand = new DelegateCommand<object>(UnregisterHotkey, CanUnregisterHotkey);
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
        }

        /// <summary>
        /// Test that verifies if UnregisterHotkey command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanUnregisterHotkey(object parameter)
        {
            return true;
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
                OnPropertyChanged(this.Property(() => Hotkey));
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
                OnPropertyChanged(this.Property(() => Hotkey));
            }
        }

        /// <summary>
        /// String representation of hotkey used
        /// </summary>
        public string Hotkey
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

        #endregion
    }
}
