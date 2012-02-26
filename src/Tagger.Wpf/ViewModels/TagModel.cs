using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Tagger.WinAPI;
using Utils.Diagnostics;
using Utils.Extensions;
using Utils.Prism;
using RECT = Tagger.WinAPI.NativeAPI.RECT;

namespace Tagger.ViewModels
{
    /// <summary>
    /// View model for tag window
    /// </summary>
    public class TagModel : ViewModelBase
    {
        /// <summary>
        /// Initializes new instance of tag view model
        /// </summary>
        /// <param name="tagWindow">Tag window itself</param>
        /// <param name="host">Host window the tag belongs to</param>
        /// <param name="tagRender">Tag render parameters</param>
        /// <param name="settingsWindow">Tag settings window that is used to change render parameters</param>
        public TagModel(Window tagWindow, IntPtr host, TagRender tagRender)
        {
            Check.Require(tagWindow != null, "Tag window must not be null");
            Check.Require(host != IntPtr.Zero, "Host window must not be null");
            Check.Require(tagRender != null, "Tag render parameters must not be null");

            // Initialize properties
            this.TagWindow = tagWindow;
            this.HostWindow = host;
            this.TagRender = tagRender;
            this.ProcessListner = new ProcessListner(this.HostWindow);
            this.ToggleSettingsCommand = new DelegateCommand<object>(delegate
            {
                if (this.SettingsWindow != null)
                {
                    this.SettingsWindow.ToggleVisibility();
                }
            });

            // Subscriptions
            this.ProcessListner.Moved += delegate { this.UpdateTagWindowPosition(this.TagWindow.Width); };

            this.TagRender.PropertyChanged += (sender, args) => this.UpdateTagWindowPosition(this.TagWindow.Width);
            this.TagWindow.SizeChanged += (sender, args) => this.UpdateTagWindowPosition(args.NewSize.Width);
            this.TagWindow.MouseRightButtonUp += delegate { this.ToggleSettingsCommand.Execute(null); };

            // Initialize tag window
            this.InitializeWindow();
        }

        /// <summary>
        /// Initialize tag window to use current view model
        /// </summary>
        private void InitializeWindow()
        {
            this.TagWindow.DataContext = this;
            this.TagWindow.SetOwner(this.HostWindow);
            this.TagWindow.Show();
        }

        /// <summary>
        /// Clean up allocated listner
        /// </summary>
        protected override void OnDisposeManaged()
        {
            base.OnDisposeManaged();
            this.ProcessListner.Dispose();
        }

        /// <summary>
        /// Update tag window position based on host window position
        /// </summary>
        /// <param name="width">Width of the tag window (need to specify explicitly for size changing events related to changed settings)</param>
        private void UpdateTagWindowPosition(double width)
        {
            RECT clientArea = this.GetHostWindowClientArea();

            this.TagWindow.Top = clientArea.Top + this.TagRender.OffsetTop;
            this.TagWindow.Left = clientArea.Right - width - this.TagRender.OffsetRight;
        }

        /// <summary>
        /// Gets host window client area rectangle
        /// </summary>
        /// <returns>
        /// Rectangle used to render host window content
        /// </returns>
        private RECT GetHostWindowClientArea()
        {
            RECT sizes;
            bool success = NativeAPI.GetWindowRect(this.TagWindow.GetOwner(), out sizes);

            if (!success)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            var zero = NativeAPI.SendMessage(this.TagWindow.GetOwner(), NativeAPI.WM_NCCALCSIZE, 0, ref sizes);
            Check.Ensure(zero == 0);

            return sizes;
        }

        /// <summary>
        /// Tag window itself
        /// </summary>
        public Window TagWindow { get; set; }

        /// <summary>
        /// Host window this tag belongs to
        /// </summary>
        public IntPtr HostWindow { get; set; }

        /// <summary>
        /// Tag render parameters
        /// </summary>
        /// <remarks>
        /// Used by WPF bindings
        /// </remarks>
        public TagRender TagRender { get; private set; }

        /// <summary>
        /// Tag context 
        /// </summary>
        public Window SettingsWindow { get; internal set; }

        /// <summary>
        /// Listner for events that happens in another process
        /// </summary>
        private ProcessListner ProcessListner { get; set; }

        /// <summary>
        /// Shows or hides settings window that is associated with the tag
        /// </summary>
        public DelegateCommand<object> ToggleSettingsCommand { get; private set; }
    }
}
