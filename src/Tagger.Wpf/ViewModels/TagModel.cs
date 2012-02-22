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
    public class TagModel: ViewModelBase
    {
        /// <summary>
        /// Initializes new instance of tag view model
        /// </summary>
        /// <param name="tagWindow">Tag window itself</param>
        /// <param name="host">Host window the tag belongs to</param>
        /// <param name="tagRender">Tag render parameters</param>
        /// <param name="settingsWindow">Tag settings window that is used to change render parameters</param>
        public TagModel(Window tagWindow, IntPtr host, TagRender tagRender, Window settingsWindow)
        {
            Check.Require(tagWindow != null, "Tag window must not be null");
            Check.Require(host != IntPtr.Zero, "Host window must not be null");
            Check.Require(tagRender != null, "Tag render parameters must not be null");
            Check.Require(settingsWindow != null, "Settings window must not be null");

            // Initialize properties
            this.TagWindow = tagWindow;
            this.HostWindow = host;
            this.TagRender = tagRender;
            this.SettingsWindow = settingsWindow;
            this.ToggleSettingsCommand = new DelegateCommand<object>(obj => this.SettingsWindow.ToggleVisibility());

            // Subscribe to host window movements
            this.WindowMovedListner = new WindowMovedListner(this.HostWindow);
            this.WindowMovedListner.Moved += delegate { this.UpdateTagWindowPosition(); };

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
            this.UpdateTagWindowPosition();

            this.TagWindow.MouseRightButtonUp += delegate
            {
                this.ToggleSettingsCommand.Execute(null);
            };
        }

        /// <summary>
        /// Clean up allocated listner
        /// </summary>
        protected override void OnDisposeManaged()
        {
            base.OnDisposeManaged();
            this.WindowMovedListner.Dispose();
        }

        /// <summary>
        /// Update tag window position based on host window position
        /// </summary>
        private void UpdateTagWindowPosition()
        {
            RECT clientArea = this.GetHostWindowClientArea();

            this.TagWindow.Top = clientArea.Top;
            this.TagWindow.Left = clientArea.Right - this.TagWindow.Width;
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
        public Window SettingsWindow { get; private set; }

        /// <summary>
        /// Listner that fires events on window moves
        /// </summary>
        private WindowMovedListner WindowMovedListner { get; set; }

        /// <summary>
        /// Shows or hides settings window that is associated with the tag
        /// </summary>
        public DelegateCommand<object> ToggleSettingsCommand { get; private set; }
    }
}
