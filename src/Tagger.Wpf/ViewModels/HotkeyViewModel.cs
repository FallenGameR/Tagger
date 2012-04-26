//-----------------------------------------------------------------------
// <copyright file="HotkeyViewModel.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Tagger.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;

    using Microsoft.Practices.Prism.Commands;

    using Tagger.Wpf.Views;

    using Utils.Diagnostics;
    using Utils.Extensions;
    using Utils.Prism;

    /// <summary>
    /// View model for global windows hotkey setting
    /// </summary>
    public class HotkeyViewModel : ViewModelBase
    {
        /// <summary>
        /// Status field
        /// </summary>
        private string m_Status;

        /// <summary>
        /// Check if key is a modifier (Ctrl, Alt, Shift)
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>true if key is a modifier</returns>
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

        /// <summary>
        /// Initializes new instance of HotkeyViewModel class
        /// </summary>
        /// <param name="control">Control to be bound to</param>
        public HotkeyViewModel(HotkeyView control)
        {
            Check.Require(control != null, "Hotkey control should not be null");

            // Initialize properties
            this.Key = Key.None;
            this.ModifierKeys = ModifierKeys.None;
            this.GlobalHotkey = null;
            this.HotkeyHandler = control.Handler;
            this.UnregisterHotkeyCommand = new DelegateCommand<object>(this.UnregisterHotkey, this.CanUnregisterHotkey);

            // Bind to view
            control.DataContext = this;
            control.ShortcutTxt.PreviewKeyDown += this.KeyDownHandler;
        }

        /// <summary>
        /// Unregister previously registered global hotkey
        /// </summary>
        public DelegateCommand<object> UnregisterHotkeyCommand { get; private set; }

        /// <summary>
        /// Modifier keys used for hotkey 
        /// </summary>
        public ModifierKeys ModifierKeys { get; set; }

        /// <summary>
        /// Key used in hotkey
        /// </summary>
        public Key Key { get; set; }

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

        /// <summary>
        /// Global hotkey object that is used to watch for hotkey event
        /// </summary>
        private GlobalHotkey GlobalHotkey { get; set; }

        /// <summary>
        /// Handler for the global hotkey
        /// </summary>
        private Action HotkeyHandler { get; set; }

        /// <summary>
        /// String representation of hotkey used
        /// </summary>
        private string HotkeyText
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
        /// Cleanup resources
        /// </summary>
        protected override void OnDisposeManaged()
        {
            base.OnDisposeManaged();

            if (this.GlobalHotkey != null)
            {
                this.GlobalHotkey.Dispose();
                this.GlobalHotkey = null;
            }
        }

        /// <summary>
        /// RegisterHotkey command handler
        /// </summary>
        public void RegisterHotkey()
        {
            // Guard for no handler
            if (this.HotkeyHandler == null)
            {
                this.Status = "No handler";
                return;
            }

            // Guard for not set key
            if (this.Key == Key.None)
            {
                this.Status = "None";
                return;
            }

            // Unregister previous hotkey
            if (this.CanUnregisterHotkey(null))
            {
                this.UnregisterHotkeyCommand.Execute(null);
            }

            // Try to register new global hotkey and update status
            try
            {
                this.GlobalHotkey = new GlobalHotkey(this.ModifierKeys, this.Key);
                this.GlobalHotkey.KeyPressed += delegate { this.HotkeyHandler(); };
                this.Status = this.HotkeyText;
            }
            catch (Win32Exception winEx)
            {
                this.Status = string.Format("Failed to register '{0}' - {1}", this.HotkeyText, winEx.Message);
            }

            // Update command can execute status
            this.OnDelegateCommandsCanExecuteChanged();
        }

        /// <summary>
        /// UnregisterHotkey command handler
        /// </summary>
        private void UnregisterHotkey(object parameter)
        {
            Check.Require(this.CanUnregisterHotkey(parameter), "You are not able to unregister hotkey");

            this.GlobalHotkey.Dispose();
            this.GlobalHotkey = null;
            this.Status = "None";

            this.OnDelegateCommandsCanExecuteChanged();
        }

        /// <summary>
        /// Test that verifies if UnregisterHotkey command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanUnregisterHotkey(object parameter)
        {
            return this.GlobalHotkey != null;
        }

        /// <summary>
        /// Handler for key down event
        /// </summary>
        /// <remarks>
        /// Used instead of a binding 
        /// </remarks>
        /// <param name="sender">Event sender</param>
        /// <param name="ea">Event arguments</param>
        private void KeyDownHandler(object sender, KeyEventArgs ea)
        {
            // Fetch the actual shortcut key
            var key = (ea.Key == Key.System) ? ea.SystemKey : ea.Key;

            // Ignore modifier keys
            if (HotkeyViewModel.IsModifierKey(key)) { return; }

            // Set view model properties
            this.ModifierKeys = Keyboard.Modifiers;
            this.Key = key;
            this.RegisterHotkey();

            // The text box grabs all input
            ea.Handled = true;
        }
    }
}

