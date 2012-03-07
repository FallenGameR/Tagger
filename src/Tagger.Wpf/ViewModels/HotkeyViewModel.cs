using System.ComponentModel;
using System.Windows.Input;
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

        private ModifierKeys m_ModifierKeys;
        private Key m_Key;
        private string m_Status;
        private GlobalHotkey m_GlobalHotkey;

        #endregion

        #region Constructors

        public HotkeyViewModel()
        {
            // Initialize fields
            this.m_GlobalHotkey = null;
            this.UnregisterHotkeyCommand = new DelegateCommand<object>(this.UnregisterHotkey, this.CanUnregisterHotkey);

            // Restore previous settings state
            this.ModifierKeys = (ModifierKeys)Settings.Default.Hotkey_Modifiers;
            this.Key = (Key)Settings.Default.Hotkey_Keys;
            
            // Restore hotkey if possible
            this.RegisterHotkey();

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

            control.ShortcutTxt.PreviewKeyDown += (object sender, KeyEventArgs e) =>
            {
                // Fetch the actual shortcut key
                var key = (e.Key == Key.System) ? e.SystemKey : e.Key;

                // Ignore modifier keys
                if (HotkeyViewModel.IsModifierKey(key)) { return; }

                // Set view model properties
                this.ModifierKeys = Keyboard.Modifiers;
                this.Key = key;
                this.RegisterHotkey();

                // The text box grabs all input
                e.Handled = true;
            };
        }

        private static bool IsModifierKey(Key key)
        {
            return key == Key.LeftShift
                || key == Key.RightShift
                || key == Key.LeftCtrl
                || key == Key.RightCtrl
                || key == Key.LeftAlt
                || key == Key.RightAlt
                || key == Key.LWin
                || key == Key.RWin;
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

        /// <summary>
        /// RegisterHotkey command handler
        /// </summary>
        private void RegisterHotkey()
        {
            if (this.Key == Key.None )
            {
                this.Status = "None";
                return;
            }

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
                this.Status = this.HotkeyText;
            }
            catch (Win32Exception winEx)
            {
                this.Status = string.Format( "Failed to register '{0}' - {1}", this.HotkeyText, winEx.Message);
            }

            // Update command can execute status
            this.OnDelegateCommandsCanExecuteChanged();
        }

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
            Check.Require(this.CanUnregisterHotkey(parameter));

            this.m_GlobalHotkey.Dispose();
            this.m_GlobalHotkey = null;

            this.Status = "None";
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
        public ModifierKeys ModifierKeys
        {
            get
            {
                return this.m_ModifierKeys;
            }
            set
            {
                this.m_ModifierKeys = value;
                OnPropertyChanged(this.Property(() => ModifierKeys));
                OnDelegateCommandsCanExecuteChanged();
            }
        }

        /// <summary>
        /// Key used in hotkey
        /// </summary>
        public Key Key
        {
            get
            {
                return this.m_Key;
            }
            set
            {
                this.m_Key = value;
                OnPropertyChanged(this.Property(() => Key));
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

                if (this.ModifierKeys.HasFlag(ModifierKeys.Control))
                {
                    hotkey += "Ctrl+";
                }
                if (this.ModifierKeys.HasFlag(ModifierKeys.Shift))
                {
                    hotkey += "Shift+";
                }
                if (this.ModifierKeys.HasFlag(ModifierKeys.Alt))
                {
                    hotkey += "Alt+";
                }
                if (this.ModifierKeys.HasFlag(ModifierKeys.Windows))
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
