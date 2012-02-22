using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.Prism;
using Utils.Extensions;
using System.Windows.Media;
using Microsoft.Practices.Prism.Commands;
using Utils.Diagnostics;

namespace Tagger.ViewModels
{
    /// <summary>
    /// View model for tag window
    /// </summary>
    public class TagModel: ViewModelBase
    {
        /// <summary>
        /// Initializes new instance of tag view model
        /// </summary>
        /// <param name="tagContext">Tag context that this settings view model belongs to</param>
        /// <param name="tagRender">Tag render parameters</param>
        public TagModel(TagContext tagContext, TagRender tagRender)
        {
            Check.Require(tagContext != null, "Context must not be null");
            Check.Require(tagRender != null, "Render parameters must not be null");

            this.TagContext = tagContext;
            this.TagRender = tagRender;

            this.ToggleSettingsCommand = new DelegateCommand<object>(this.ToggleSettings, this.CanToggleSettings);
        }

        #region Properties

        /// <summary>
        /// Tag context 
        /// </summary>
        public TagContext TagContext { get; private set; }

        /// <summary>
        /// Tag render settings
        /// </summary>
        public TagRender TagRender { get; private set; }

        #endregion

        #region Command - ToggleSettings

        /// <summary>
        /// Shows or hides settings window that is associated with the tag
        /// </summary>
        public DelegateCommand<object> ToggleSettingsCommand { get; private set; }

        /// <summary>
        /// ToggleSettings command handler
        /// </summary>
        private void ToggleSettings(object parameter)
        {
            this.TagContext.SettingsWindow.ToggleVisibility();
        }

        /// <summary>
        /// Test that verifies if ToggleSettings command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanToggleSettings(object parameter)
        {
            return true;
        }

        #endregion
    }
}
