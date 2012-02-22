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
        /// Listner that fires events on window moves
        /// </summary>
        private WindowMovedListner m_WindowMovedListner;


        private Window window;
        private IntPtr host;

        /// <summary>
        /// Initializes new instance of tag view model
        /// </summary>
        /// <param name="tagContext">Tag context that this settings view model belongs to</param>
        /// <param name="tagRender">Tag render parameters</param>
        /// <param name="host">Host window this tag belong to</param>
        /// <param name="viewModel">View model with all settings needed for tag render</param>
        public TagModel(Window window, IntPtr host, TagRender tagRender, TagContext tagContext)
        {
            Check.Require(tagContext != null, "Context must not be null");
            Check.Require(tagRender != null, "Render parameters must not be null");

            this.TagContext = tagContext;
            this.TagRender = tagRender;
            this.TagWindow = window;

            this.ToggleSettingsCommand = new DelegateCommand<object>(this.ToggleSettings, this.CanToggleSettings);

            // Bind to view model
            window.DataContext = this;

            // Set window owner so that the tag would always be on top of it
            window.SetOwner(host);

            // Subscribe to the tagged window movements
            this.m_WindowMovedListner = new WindowMovedListner(host);
            this.m_WindowMovedListner.Moved += delegate { this.UpdateTagPosition(); };

            // Show tag window in the right position
            window.Show();
            this.UpdateTagPosition();


            window.MouseRightButtonUp += delegate
            {
                this.ToggleSettingsCommand.Execute(null);
            };
        }

        protected override void OnDisposeManaged()
        {
            base.OnDisposeManaged();

            if (this.m_WindowMovedListner != null)
            {
                this.m_WindowMovedListner.Dispose();
            }
        }



        /// <summary>
        /// Update tag position based on host window position
        /// </summary>
        private void UpdateTagPosition()
        {
            RECT clientArea = this.GetHostClientArea();

            this.TagWindow.Top = clientArea.Top;
            this.TagWindow.Left = clientArea.Right - this.TagWindow.Width;
        }

        /// <summary>
        /// Gets host client area rectangle
        /// </summary>
        /// <returns>
        /// Rectangle that borders host window content
        /// </returns>
        private RECT GetHostClientArea()
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

        #region Properties

        /// <summary>
        /// Tag context 
        /// </summary>
        public TagContext TagContext { get; private set; }

        /// <summary>
        /// Tag render settings
        /// </summary>
        public TagRender TagRender { get; private set; }

        #endregion

        #region Command - ToggleSettings

        /// <summary>
        /// Shows or hides settings window that is associated with the tag
        /// </summary>
        public DelegateCommand<object> ToggleSettingsCommand { get; private set; }

        /// <summary>
        /// ToggleSettings command handler
        /// </summary>
        private void ToggleSettings(object parameter)
        {
            this.TagContext.SettingsWindow.ToggleVisibility();
        }

        /// <summary>
        /// Test that verifies if ToggleSettings command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanToggleSettings(object parameter)
        {
            return true;
        }

        #endregion

        public Window TagWindow { get; set; }
    }
}
