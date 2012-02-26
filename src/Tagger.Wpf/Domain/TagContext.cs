using System;
using Tagger.ViewModels;
using Tagger.Wpf;
using Tagger.Wpf.Windows;

namespace Tagger
{
    /// <summary>
    /// Context of a tag
    /// </summary>
    public sealed class TagContext : IDisposable
    {
        /// <summary>
        /// Clean up all resources
        /// </summary>
        public void Dispose()
        {
            // TODO: Dispose in reverse order - tagModel should unregister event handlers before tagRender is disposed

            if (this.HostListner != null )
            {
                this.HostListner.Dispose();
            }
            if (this.TagRender != null)
            {
                this.TagRender.Dispose();
            }
            if (this.TagWindow != null)
            {
                // TODO: How to force it?
                this.TagWindow.Close();
            }
            if (this.TagModel != null)
            {
                this.TagModel.Dispose(); 
            }
            if (this.SettingsWindow != null)
            {
                // TODO: How to force it?
                this.SettingsWindow.Close();
            }
            if (this.SettingsModel != null)
            {
                this.SettingsModel.Dispose();
            }
        }

        /// <summary>
        /// Window handle that is tagged
        /// </summary>
        public IntPtr HostWindow { get; internal set; }

        /// <summary>
        /// Listner for events that happens in host process
        /// </summary>
        public ProcessListner HostListner { get; internal set; }

        /// <summary>
        /// Tag render view model that is shared between settings and tag windows
        /// </summary>
        public TagRender TagRender { get; internal set; }

        /// <summary>
        /// Tag window itself
        /// </summary>
        public TagWindow TagWindow { get; internal set; }

        /// <summary>
        /// Tag window view model that controls the tag window
        /// </summary>
        public TagModel TagModel { get; internal set; }
        /// <summary>
        /// Settings window that setup tag appearance
        /// </summary>
        public SettingsWindow SettingsWindow { get; internal set; }

        /// <summary>
        /// Settings window view model that controls the settings window
        /// </summary>
        public SettingsModel SettingsModel { get; internal set; }
    }
}
