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
        /// <param name="context">Tag context that this settings view model belongs to</param>
        public SettingsModel(Window window, IntPtr host)
        {
            //Check.Require(context != null, "Context must not be null");

            this.TagRender = new TagRender();

            this.HideSettingsCommand = new DelegateCommand<object>(this.HideSettings, this.CanHideSettings);

            window.DataContext = this;
            window.SetOwner(host);
            window.Show();
            window.Closing += (sender, args) =>
            {
                args.Cancel = true;
                window.Hide();
            };
        }

        #region Properties


        /// <summary>
        /// Tag render settings
        /// </summary>
        public TagRender TagRender { get; private set; }

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

        public TagModel GetTagModel(TagContext context)
        {
            return new TagModel( context, this.TagRender );
        }
    }
}
