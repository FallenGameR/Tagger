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
            this.TagRender = new TagRender();

            this.SettingsWindow = new SettingsWindow();
            this.SettingsModel = new SettingsModel(this.SettingsWindow, this.HostWindow, this.TagRender);

            this.TagWindow = new TagWindow();
            this.TagModel = new TagModel(this.TagWindow, this.HostWindow, this.TagRender, this);

            this.SettingsWindow.Focus();
        }

        /// <summary>
        /// Window handle that is tagged
        /// </summary>
        public IntPtr HostWindow { get; private set; }

        /// <summary>
        /// Tag render view model that is shared between settings and tag windows
        /// </summary>
        public TagRender TagRender { get; set; }

        /// <summary>
        /// Settings window that setup tag appearance
        /// </summary>
        public SettingsWindow SettingsWindow { get; private set; }

        /// <summary>
        /// Settings window view model that controls the settings window
        /// </summary>
        public SettingsModel SettingsModel { get; private set; }

        /// <summary>
        /// Tag window itself
        /// </summary>
        public TagWindow TagWindow { get; private set; }

        /// <summary>
        /// Tag window view model that controls the tag window
        /// </summary>
        public TagModel TagModel { get; private set; }
    }
}
