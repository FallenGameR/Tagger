using System.ComponentModel;
//using System.Windows.Forms;
//using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Tagger.Properties;
using Tagger.Wpf;
using Tagger.Wpf.Views;
using Utils.Diagnostics;
using Utils.Extensions;
using Utils.Prism;

namespace Tagger.ViewModels
{
    /// <summary>
    /// View model for global windows hotkey setting
    /// </summary>
    public class HotkeyViewModel : ViewModelBase
    {
        #region Fields

        private System.Windows.Input.ModifierKeys m_ModifierKeys;
        private System.Windows.Forms.Keys m_Key;
        private string m_Status;
        private GlobalHotkey m_GlobalHotkey;

        #endregion

        #region Constructors

        public HotkeyViewModel()
        {
            // Initialize fields
            this.m_GlobalHotkey = null;
            this.RegisterHotkeyCommand = new DelegateCommand<object>(this.RegisterHotkey, this.CanRegisterHotkey);
            this.UnregisterHotkeyCommand = new DelegateCommand<object>(this.UnregisterHotkey, this.CanUnregisterHotkey);

            // Restore previous settings state
            this.ModifierKeys = (System.Windows.Input.ModifierKeys)Settings.Default.Hotkey_Modifiers;
            this.Key = (System.Windows.Forms.Keys)Settings.Default.Hotkey_Keys;

            // Restore hotkey if possible
            if (this.CanRegisterHotkey(null))
            {
                this.RegisterHotkeyCommand.Execute(null);
            }
            else
            {
                this.Status = "No hotkey registered";
            }

            // Save settings on program deactivation (app exit included)
            App.Current.Deactivated += delegate
            {
                Settings.Default.Hotkey_Modifiers = (int)this.ModifierKeys;
                Settings.Default.Hotkey_Keys = (int)this.Key;
                Settings.Default.Save();
            };
        }

        #endregion

        public void BindToView(HotkeyControl control)
        {
            control.DataContext = this;
            App.Current.Exit += delegate { this.Dispose(); };

            control.ShortcutTxt.PreviewKeyDown += (object sender, System.Windows.Input.KeyEventArgs e) =>
            {
                // Fetch the actual shortcut key
                var key = (e.Key == System.Windows.Input.Key.System) ? e.SystemKey : e.Key;

                // Ignore modifier keys
                if (IsModifierKey(key)) { return; }

                // Set view model properties
                this.ModifierKeys = System.Windows.Input.Keyboard.Modifiers;
                this.Key = (System.Windows.Forms.Keys)System.Windows.Input.KeyInterop.VirtualKeyFromKey(key);

                // The text box grabs all input
                e.Handled = true;
            };
        }

        private static bool IsModifierKey(System.Windows.Input.Key key)
        {
            return key == System.Windows.Input.Key.LeftShift
                || key == System.Windows.Input.Key.RightShift
                || key == System.Windows.Input.Key.LeftCtrl
                || key == System.Windows.Input.Key.RightCtrl
                || key == System.Windows.Input.Key.LeftAlt
                || key == System.Windows.Input.Key.RightAlt
                || key == System.Windows.Input.Key.LWin
                || key == System.Windows.Input.Key.RWin;
        }

        #region IDisposable Members

        protected override void OnDisposeManaged()
        {
            base.OnDisposeManaged();

            if (this.m_GlobalHotkey != null)
            {
                this.m_GlobalHotkey.Dispose();
                this.m_GlobalHotkey = null;
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
            if (this.m_GlobalHotkey != null)
            {
                this.UnregisterHotkeyCommand.Execute(null);
            }

            // Try to register new global hotkey and update status
            try
            {
                this.m_GlobalHotkey = new GlobalHotkey(this.ModifierKeys, this.Key);
                this.m_GlobalHotkey.KeyPressed += (s, a) => RegistrationManager.GlobalHotkeyHandle();
                this.Status = "Registered hotkey: " + HotkeyText;
            }
            catch (Win32Exception winEx)
            {
                this.Status = "Failed to register hotkey: " + winEx.Message;
            }

            // Update command can execute status
            this.OnDelegateCommandsCanExecuteChanged();
        }

        /// <summary>
        /// Test that verifies if RegisterHotkey command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanRegisterHotkey(object parameter)
        {
            return this.Key != System.Windows.Forms.Keys.None;
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

            this.m_GlobalHotkey.Dispose();
            this.m_GlobalHotkey = null;

            this.Status = "Hotkey unregistered";
            this.OnDelegateCommandsCanExecuteChanged();
        }

        /// <summary>
        /// Test that verifies if UnregisterHotkey command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanUnregisterHotkey(object parameter)
        {
            return this.m_GlobalHotkey != null;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Modifier keys used for hotkey 
        /// </summary>
        public System.Windows.Input.ModifierKeys ModifierKeys
        {
            get
            {
                return this.m_ModifierKeys;
            }
            set
            {
                this.m_ModifierKeys = value;
                OnPropertyChanged(this.Property(() => ModifierKeys));
                OnPropertyChanged(this.Property(() => HotkeyText));
                OnDelegateCommandsCanExecuteChanged();
            }
        }

        /// <summary>
        /// Key used in hotkey
        /// </summary>
        public System.Windows.Forms.Keys Key
        {
            get
            {
                return this.m_Key;
            }
            set
            {
                this.m_Key = value;
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

                if (this.ModifierKeys.HasFlag(System.Windows.Input.ModifierKeys.Control))
                {
                    hotkey += "Ctrl+";
                }
                if (this.ModifierKeys.HasFlag(System.Windows.Input.ModifierKeys.Shift))
                {
                    hotkey += "Shift+";
                }
                if (this.ModifierKeys.HasFlag(System.Windows.Input.ModifierKeys.Alt))
                {
                    hotkey += "Alt+";
                }
                if (this.ModifierKeys.HasFlag(System.Windows.Input.ModifierKeys.Windows))
                {
                    hotkey += "Win+";
                }

                return hotkey + this.Key.ToString();
            }
        }

        /// <summary>
        /// Hotkey registration status
        /// </summary>
        public string Status
        {
            get
            {
                return this.m_Status;
            }
            private set
            {
                this.m_Status = value;
                OnPropertyChanged(this.Property(() => Status));
            }
        }

        #endregion
    }
}
