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
        private string m_Status;

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

        public HotkeyViewModel()
        {
            this.GlobalHotkey = null;
            this.UnregisterHotkeyCommand = new DelegateCommand<object>(this.UnregisterHotkey, this.CanUnregisterHotkey);
        }

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

        #region IDisposable Members

        protected override void OnDisposeManaged()
        {
            base.OnDisposeManaged();

            if (this.GlobalHotkey != null)
            {
                this.GlobalHotkey.Dispose();
                this.GlobalHotkey = null;
            }
        }

        #endregion

        /// <summary>
        /// RegisterHotkey command handler
        /// </summary>
        public void RegisterHotkey()
        {
            if (this.Key == Key.None )
            {
                this.Status = "None";
                return;
            }

            // Unregister previous hotkey if needed
            if (this.GlobalHotkey != null)
            {
                this.UnregisterHotkeyCommand.Execute(null);
            }

            // Try to register new global hotkey and update status
            try
            {
                this.GlobalHotkey = new GlobalHotkey(this.ModifierKeys, this.Key);
                this.GlobalHotkey.KeyPressed += (s, a) => RegistrationManager.GlobalHotkeyHandle();
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

            this.GlobalHotkey.Dispose();
            this.GlobalHotkey = null;
            this.Status = "None";
        }

        /// <summary>
        /// Test that verifies if UnregisterHotkey command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanUnregisterHotkey(object parameter)
        {
            return this.GlobalHotkey != null;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Global hotkey object that is used to watch for hotkey event
        /// </summary>
        private GlobalHotkey GlobalHotkey { get; set; }

        /// <summary>
        /// Modifier keys used for hotkey 
        /// </summary>
        public ModifierKeys ModifierKeys { get; set; }

        /// <summary>
        /// Key used in hotkey
        /// </summary>
        public Key Key { get; set; }

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
