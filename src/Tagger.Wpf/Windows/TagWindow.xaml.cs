using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using Tagger.ViewModels;
using Tagger.WinAPI;
using Utils.Diagnostics;
using Utils.Extensions;
using RECT = Tagger.WinAPI.NativeAPI.RECT;

namespace Tagger.Wpf
{
    /// <summary>
    /// Interaction logic for OverlayWindow.xaml
    /// </summary>
    public sealed partial class TagWindow : Window
    {
        /// <summary>
        /// Listner that fires events on window moves
        /// </summary>
        private WindowMovedListner m_WindowMovedListner;

        /// <summary>
        /// Constructor that is used by the studio designer
        /// </summary>
        public TagWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes and shows new instance of tag window 
        /// </summary>
        /// <param name="host">Host window this tag belong to</param>
        /// <param name="viewModel">View model with all settings needed for tag render</param>
        public TagWindow(IntPtr host, TagModel viewModel)
            : this()
        {
            // Bind to view model
            this.ViewModel = viewModel;

            // Set window owner so that the tag would always be on top of it
            this.SetOwner( host );

            // Subscribe to the tagged window movements
            this.m_WindowMovedListner = new WindowMovedListner(host);
            this.m_WindowMovedListner.Moved += delegate { this.UpdateTagPosition(); };

            // Show tag window in the right position
            this.Show();
            this.UpdateTagPosition();
        }

        /// <summary>
        /// Cleanup all allocated resources
        /// </summary>
        public void Dispose()
        {
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

            this.Top = clientArea.Top;
            this.Left = clientArea.Right - Width;
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
            bool success = NativeAPI.GetWindowRect(this.GetOwner(), out sizes);

            if (!success)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            var zero = NativeAPI.SendMessage(this.GetOwner(), NativeAPI.WM_NCCALCSIZE, 0, ref sizes);
            Check.Ensure(zero == 0);

            return sizes;
        }

        /// <summary>
        /// TODO: Show settings dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.ViewModel.ToggleSettingsCommand.Execute(null);
        }

        /// <summary>
        /// View model for the window
        /// </summary>
        private TagModel ViewModel
        {
            get { return (TagModel)this.DataContext; }
            set { this.DataContext = value; }
        }
    }
}
