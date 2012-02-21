using System;
using System.Windows;
using Tagger.Wpf;
using Tagger.Wpf.ViewModels;
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

            var settingsModel = new SettingsModel();
            this.SettingsWindow = new SettingsWindow(host, settingsModel);

            // TODO: Add visible property, bind it to settings
            var tagModel = settingsModel.GetTagModel();
            this.TagWindow = new TagWindow(host, tagModel);
        }

        /// <summary>
        /// Window handle that is tagged
        /// </summary>
        public IntPtr HostWindow { get; private set; }

        /// <summary>
        /// Tag window itself
        /// </summary>
        public TagWindow TagWindow { get; private set; }

        /// <summary>
        /// Settings window that setup tag appearance
        /// </summary>
        public SettingsWindow SettingsWindow { get; private set; }

        /// <summary>
        /// Toggles tag window visibility
        /// </summary>
        public void ToggleTagVisibility()
        {
            if (this.TagWindow.Visibility == Visibility.Visible)
            {
                this.TagWindow.Hide();
            }
            else
            {
                this.TagWindow.Show();
            }
        }
    }
}
