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
        /// Update tag window position based on host window position
        /// </summary>
        /// <param name="width">Width of the tag window (need to specify explicitly for size changing events related to changed settings)</param>
        internal void UpdateTagWindowPosition(double width)
        {
            var clientArea = WindowSizes.GetClientArea(this.TagWindow.GetOwner());

            this.TagWindow.Top = clientArea.Top + this.TagRender.OffsetTop;
            this.TagWindow.Left = clientArea.Right - width - this.TagRender.OffsetRight;
        }

        /// <summary>
        /// Tag window itself
        /// </summary>
        public Window TagWindow { get; internal set; }

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
