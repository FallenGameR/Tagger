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
        /// Initializes new instance of tag context, shows tag and settings windows
        /// </summary>
        /// <param name="host">Window host that is tagged</param>
        public TagContext(IntPtr host)
        {
            Check.Require(host != IntPtr.Zero, "Host should not be zero");

            this.HostWindow = host;

            this.SettingsWindow = new SettingsWindow();
            this.SettingsModel = new SettingsModel(this.SettingsWindow, this.HostWindow);

            var tagModel = this.SettingsModel.GetTagModel(this);
            this.TagWindow = new TagWindow(host, tagModel);

            this.SettingsWindow.Focus();
        }

        /// <summary>
        /// Window handle that is tagged
        /// </summary>
        public IntPtr HostWindow { get; private set; }

        /// <summary>
        /// Settings window that setup tag appearance
        /// </summary>
        public SettingsWindow SettingsWindow { get; private set; }

        /// <summary>
        /// Settings window view model that controls the window
        /// </summary>
        public SettingsModel SettingsModel { get; private set; }

        /// <summary>
        /// Tag window itself
        /// </summary>
        public TagWindow TagWindow { get; private set; }

    }
}
