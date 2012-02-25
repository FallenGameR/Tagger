using System;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Utils.Diagnostics;
using Utils.Extensions;
using Utils.Prism;
using Tagger.WinAPI;

namespace Tagger.ViewModels
{
    /// <summary>
    /// View model for the settings window
    /// </summary>
    public class SettingsModel : ViewModelBase
    {
        /// <summary>
        /// Initializes new instance of settings view model
        /// </summary>
        public SettingsModel(Window settingsWindow, IntPtr host, TagRender tagRender)
        {
            Check.Require(settingsWindow != null, "Window must not be null");
            Check.Require(host != IntPtr.Zero, "Host must not be zero");
            Check.Require(tagRender != null, "Tag render object must not be null");

            this.SettingsWindow = settingsWindow;
            this.HostWindow = host;
            this.TagRender = tagRender;
            this.HideSettingsCommand = new DelegateCommand<object>(this.HideSettingsHandler);
            this.SaveAsDefaultCommand = new DelegateCommand<object>(obj => this.TagRender.SaveAsDefault());
            this.LoadFromDefaultCommand = new DelegateCommand<object>(obj => this.TagRender.LoadFromDefault());

            this.InitializeWindow();
        }

        /// <summary>
        /// Initializes settings window to use current view model
        /// </summary>
        private void InitializeWindow()
        {
            this.SettingsWindow.DataContext = this;
            this.SettingsWindow.SetOwner(this.HostWindow);
            this.SettingsWindow.Show();

            this.SettingsWindow.Closing += (sender, args) =>
            {
                args.Cancel = true;
                this.SettingsWindow.Hide();
            };
        }

        /// <summary>
        /// Handler for hide settings window command
        /// </summary>
        /// <remarks>
        /// Restores focus back to the host window
        /// </remarks>
        private void HideSettingsHandler(object obj)
        {
            this.SettingsWindow.ToggleVisibility();
            NativeAPI.SetForegroundWindow(this.HostWindow);
        }

        /// <summary>
        /// Settings window this view model controls
        /// </summary>
        public Window SettingsWindow { get; private set; }

        /// <summary>
        /// Tag render settings
        /// </summary>
        /// <remarks>
        /// Used in WPF bindings
        /// </remarks>
        public TagRender TagRender { get; private set; }

        /// <summary>
        /// Host tagged window used that owns settings
        /// </summary>
        public IntPtr HostWindow { get; private set; }

        /// <summary>
        /// Hide settings window command
        /// </summary>
        public DelegateCommand<object> HideSettingsCommand { get; private set; }

        /// <summary>
        /// Command to save current settings as default for all new tags
        /// </summary>
        public DelegateCommand<object> SaveAsDefaultCommand { get; private set; }

        /// <summary>
        /// Command to load current settings from default values
        /// </summary>
        public DelegateCommand<object> LoadFromDefaultCommand { get; private set; }

    }
}
