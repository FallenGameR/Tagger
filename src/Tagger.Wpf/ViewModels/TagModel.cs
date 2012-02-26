using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Utils.Extensions;
using Utils.Prism;

namespace Tagger.ViewModels
{
    /// <summary>
    /// View model for tag window
    /// </summary>
    public class TagModel : ViewModelBase
    {
        /// <summary>
        /// Tag render parameters
        /// </summary>
        /// <remarks>
        /// Used by WPF bindings
        /// </remarks>
        public TagRender TagRender { get; internal set; }

        /// <summary>
        /// Shows or hides settings window that is associated with the tag
        /// </summary>
        public DelegateCommand<object> ToggleSettingsCommand { get; internal set; }

        /// <summary>
        /// Hide settings window command
        /// </summary>
        public DelegateCommand<object> HideSettingsCommand { get; internal set; }

        /// <summary>
        /// Command to save current settings as default for all new tags
        /// </summary>
        public DelegateCommand<object> SaveAsDefaultCommand { get; internal set; }

        /// <summary>
        /// Command to load current settings from default values
        /// </summary>
        public DelegateCommand<object> LoadFromDefaultCommand { get; internal set; }
    }
}
