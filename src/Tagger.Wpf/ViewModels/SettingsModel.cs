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
        /// Tag render settings
        /// </summary>
        /// <remarks>
        /// Used in WPF bindings
        /// </remarks>
        public TagRender TagRender { get; internal set; }

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
