using System;
using Tagger.ViewModels;
using Tagger.Wpf;
using Tagger.Wpf.Windows;

namespace Tagger
{
    /// <summary>
    /// Context of a tag
    /// </summary>
    /// <remarks>
    /// Note regarding event unsubscription. I've investigated possibility of a memory leak
    /// here and it looks like there should be none. We have several tag-related classes that
    /// subscribe on each other and are referenced from RegistrsationManager as TagContext 
    /// instance. We have two external events we rely on that could possibly store another 
    /// reference on tag related classes:
    /// - AccessibleEventListener that wraps WinAPI Hook and 
    /// - AutomationEventHandler that wraps UI Automation callback. 
    /// 
    /// Both these events are gracefully unsubscribed from in WindowListner class. Plus there 
    /// is an implicit event reference from RegistrsationManager via WindowListner instance
    /// (host destruction subscription). To handle correct unreferencing WindowListner 
    /// unregisters all subscribers in Dispose. 
    /// 
    /// Thus for garbage collection to succeed on tag deletion we need:
    /// - call dispose on WindowListner instance
    /// - unreference tag context from KnownTags collection
    /// 
    /// Here is some additional reading regarding events:
    /// http://www.codeproject.com/Articles/29922/Weak-Events-in-C
    /// http://diditwith.net/PermaLink,guid,aacdb8ae-7baa-4423-a953-c18c1c7940ab.aspx
    /// http://msdn.microsoft.com/en-us/library/ms228976.aspx
    /// </remarks>
    public sealed class TagContext : IDisposable
    {
        /// <summary>
        /// Clean up all resources
        /// </summary>
        public void Dispose()
        {
            if (this.TagWindow != null)
            {
                // TODO: How to force it?        
                this.TagWindow.Dispatcher.Invoke((Action)delegate {this.TagWindow.Close();});
            }

            if (this.SettingsWindow != null)
            {
                // TODO: How to force it?
                this.SettingsWindow.Dispatcher.Invoke((Action)delegate { this.SettingsWindow.Close(); });
            }

            if (this.TagRender != null)
            {
                this.TagRender.Dispose();
            }

            if (this.HostListner != null)
            {
                this.HostListner.Dispose();
            }
        }

        /// <summary>
        /// Window handle that is tagged
        /// </summary>
        public IntPtr HostWindow { get; internal set; }

        /// <summary>
        /// Listner for events that happens in host process
        /// </summary>
        public WindowListner HostListner { get; internal set; }

        /// <summary>
        /// Tag render view model that is shared between settings and tag windows
        /// </summary>
        public TagViewModel TagRender { get; internal set; }

        /// <summary>
        /// Tag window itself
        /// </summary>
        public TagWindow TagWindow { get; internal set; }

        /// <summary>
        /// Settings window that setup tag appearance
        /// </summary>
        public SettingsWindow SettingsWindow { get; internal set; }
    }
}
