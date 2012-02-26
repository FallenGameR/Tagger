using System;
using Tagger.ViewModels;
using Tagger.Wpf;
using Tagger.Wpf.Windows;
using Utils.Diagnostics;

namespace Tagger
{
    /// <summary>
    /// Context of a tag
    /// </summary>
    public class TagContext
    {
        /// <summary>
        /// Window handle that is tagged
        /// </summary>
        public IntPtr HostWindow { get; internal set; }

        /// <summary>
        /// Tag render view model that is shared between settings and tag windows
        /// </summary>
        public TagRender TagRender { get; internal set; }

        /// <summary>
        /// Settings window that setup tag appearance
        /// </summary>
        public SettingsWindow SettingsWindow { get; internal set; }

        /// <summary>
        /// Settings window view model that controls the settings window
        /// </summary>
        public SettingsModel SettingsModel { get; internal set; }

        /// <summary>
        /// Tag window itself
        /// </summary>
        public TagWindow TagWindow { get; internal set; }

        /// <summary>
        /// Tag window view model that controls the tag window
        /// </summary>
        public TagModel TagModel { get; internal set; }
    }
}
