using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Tagger.WinAPI;
using Tagger.Wpf;
using Tagger.Wpf.Windows;
using Utils.Diagnostics;
using Utils.Extensions;
using System.Windows.Input;

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
        /// Initializes new instance of TagContext
        /// </summary>
        /// <remarks>
        /// Windows objects are created, initialized and ready to connect to a host window
        /// </remarks>
        public TagContext()
        {
            this.TagViewModel = new TagViewModel();
            this.TagWindow = new TagWindow { DataContext = this.TagViewModel };
            this.SettingsWindow = new SettingsWindow { DataContext = this.TagViewModel };

            this.TagViewModel.ToggleSettingsCommand = new DelegateCommand<object>(o => this.SettingsWindow.ToggleVisibility());
            this.TagViewModel.HideSettingsCommand = new DelegateCommand<object>(this.HideSettingsAndRefocus);
            this.TagViewModel.PropertyChanged += (s, e) => this.RedrawTagPosition(this.TagWindow.Width);

            this.TagWindow.MouseRightButtonUp += (s, e) => this.TagViewModel.ToggleSettingsCommand.Execute(null);
            this.TagWindow.SizeChanged += (s, e) => this.RedrawTagPosition(e.NewSize.Width);

            this.SettingsWindow.Closing += this.SettingsClosingHandler;
            this.SettingsWindow.ExistingTagsComboBox.DropDownOpened += delegate
            {
                this.SettingsWindow.ExistingTagsComboBox.ItemsSource = RegistrationManager.GetExistingTags().ToList();
            };
            this.SettingsWindow.ExistingTagsComboBox.PreviewKeyDown += (sender, args) =>
            {
                var isSpace = args.Key == Key.Space;
                var isDown = (args.Key == Key.Down) || (args.Key == Key.PageDown);
                var isOpened = this.SettingsWindow.ExistingTagsComboBox.IsDropDownOpen;

                if( isSpace || (!isOpened && isDown) )
                {
                    this.SettingsWindow.ExistingTagsComboBox.IsDropDownOpen = !this.SettingsWindow.ExistingTagsComboBox.IsDropDownOpen;
                }
            };
            this.SettingsWindow.ExistingTagsComboBox.SelectionChanged += delegate
            {
                var tagLabel = (TagLabel)this.SettingsWindow.ExistingTagsComboBox.SelectedValue;
                this.TagViewModel.Text = tagLabel.Text;
                this.TagViewModel.Color = tagLabel.Color;
            };
            this.SettingsWindow.ExistingTagsComboBox.DropDownClosed += delegate
            {
                this.SettingsWindow.ExistingTagsComboBox.SelectedValue = null;
            };
        }

        /// <summary>
        /// Clean up all resources
        /// </summary>
        public void Dispose()
        {
            // Cancel hide on close for settings window
            this.SettingsWindow.Closing -= this.SettingsClosingHandler;

            // Hide and then close both views
            this.SettingsWindow.Dispatcher.Invoke((Action)delegate { this.SettingsWindow.Hide(); });
            this.TagWindow.Dispatcher.Invoke((Action)delegate { this.TagWindow.Hide(); });
            this.TagWindow.Dispatcher.Invoke((Action)delegate { this.TagWindow.Close(); });
            this.SettingsWindow.Dispatcher.Invoke((Action)delegate { this.SettingsWindow.Close(); });

            // Dispose underlying view model
            this.TagViewModel.Dispose();

            // Dispose window listner
            if (this.HostWindowListner != null)
            {
                this.HostWindowListner.Dispose();
            }
        }

        /// <summary>
        /// Attach tag to a host window
        /// </summary>
        /// <param name="hostWindow">Window handle to the host window</param>
        /// <remarks>
        /// Checks ensure that this method is called only once per tag context
        /// </remarks>
        public void AttachToHost(IntPtr hostWindow)
        {
            Check.Require(hostWindow != IntPtr.Zero, "Injected host window must not be zero");
            Check.Require(this.HostWindow == default(IntPtr), "Host window must not be initialized");

            this.HostWindow = hostWindow;

            this.TagWindow.SetOwner(this.HostWindow);
            this.TagWindow.Show();

            this.SettingsWindow.SetOwner(this.HostWindow);
            this.SettingsWindow.Show();

            this.HostWindowListner = new WindowListner(this.HostWindow);
            this.HostWindowListner.ClientAreaChanged += (s, e) => this.RedrawTagPosition(this.TagWindow.Width);
        }

        /// <summary>
        /// Hide settings window instead of close
        /// </summary>
        /// <remarks>
        /// To actually close the settings window we need to unsubscribe from such handler
        /// </remarks>
        private void SettingsClosingHandler(object sender, CancelEventArgs ea)
        {
            ea.Cancel = true;
            this.SettingsWindow.Hide();
        }

        /// <summary>
        /// Redraw tag window position when it's needed
        /// </summary>
        /// <param name="tagWindowWidth">Width of the tag window</param>
        private void RedrawTagPosition(double tagWindowWidth)
        {
            if (this.HostWindow == IntPtr.Zero)
            {
                return;
            }

            var clientArea = WindowSizes.GetClientArea(this.HostWindow);
            var newTop = clientArea.Top + this.TagViewModel.OffsetTop;
            var newLeft = clientArea.Right - tagWindowWidth - this.TagViewModel.OffsetRight;

            if (this.TagWindow.Top != newTop)
            {
                this.TagWindow.Top = newTop;
            }

            if (this.TagWindow.Left != newLeft)
            {
                this.TagWindow.Left = newLeft;
            }
        }

        /// <summary>
        /// Hide settings window and make host window focused
        /// </summary>
        private void HideSettingsAndRefocus(object sender)
        {
            if (this.HostWindow == IntPtr.Zero)
            {
                return;
            }

            this.SettingsWindow.ToggleVisibility();
            NativeAPI.SetForegroundWindow(this.HostWindow);
        }

        /// <summary>
        /// Tag render view model that is shared between settings and tag windows
        /// </summary>
        public TagViewModel TagViewModel { get; private set; }

        /// <summary>
        /// Tag window itself
        /// </summary>
        public TagWindow TagWindow { get; private set; }

        /// <summary>
        /// Settings window that setup tag appearance
        /// </summary>
        public SettingsWindow SettingsWindow { get; private set; }

        /// <summary>
        /// Window handle that is tagged
        /// </summary>
        public IntPtr HostWindow { get; private set; }

        /// <summary>
        /// Listner for events that happens in host process
        /// </summary>
        /// <remarks>
        /// On dispose unregisters all handlers from all its events. That's an unusual 
        /// behaviour but it allows to garbage collect tag context without additional clutter.
        /// </remarks>
        public WindowListner HostWindowListner { get; private set; }
    }
}
