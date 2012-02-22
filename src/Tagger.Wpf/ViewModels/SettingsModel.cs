using Utils.Prism;
using Utils.Extensions;
using Microsoft.Practices.Prism.Commands;
using System.Windows.Media;
using Utils.Diagnostics;
using System.Windows;
using System;

namespace Tagger.ViewModels
{
    public class SettingsModel : ViewModelBase
    {
        /// <summary>
        /// Initializes new instance of settings view model
        /// </summary>
        public SettingsModel(Window window, IntPtr host, TagRender tagRender)
        {
            Check.Require(window != null, "Window must not be null");
            Check.Require(host != IntPtr.Zero, "Host must not be zero");
            Check.Require(tagRender != null, "Tag render object must not be null");

            this.SettingsWindow = window;
            this.TagRender = tagRender;
            this.HideSettingsCommand = new DelegateCommand<object>(this.HideSettings, this.CanHideSettings);

            this.SettingsWindow.DataContext = this;
            this.SettingsWindow.SetOwner(host);
            this.SettingsWindow.Show();
            this.SettingsWindow.Closing += (sender, args) =>
            {
                args.Cancel = true;
                this.SettingsWindow.Hide();
            };
        }

        #region Properties

        /// <summary>
        /// Tag render settings
        /// </summary>
        public TagRender TagRender { get; private set; }

        /// <summary>
        /// Settings window this view model controls
        /// </summary>
        public Window SettingsWindow { get; private set; }

        #endregion

        #region Command - HideSettings

        /// <summary>
        /// Hide settings window command
        /// </summary>
        public DelegateCommand<object> HideSettingsCommand { get; private set; }

        /// <summary>
        /// HideSettings command handler
        /// </summary>
        private void HideSettings(object parameter)
        {
            this.SettingsWindow.ToggleVisibility();
        }

        /// <summary>
        /// Test that verifies if HideSettings command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanHideSettings(object parameter)
        {
            return true;
        }

        #endregion
    }
}
