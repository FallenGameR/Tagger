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
        /// Update tag window position based on host window position
        /// </summary>
        /// <param name="width">Width of the tag window (need to specify explicitly for size changing events related to changed settings)</param>
        internal void UpdateTagWindowPosition(double width)
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
        public Window TagWindow { get; internal set; }

        /// <summary>
        /// Host window this tag belongs to
        /// </summary>
        public IntPtr HostWindow { get; internal set; }

        /// <summary>
        /// Tag render parameters
        /// </summary>
        /// <remarks>
        /// Used by WPF bindings
        /// </remarks>
        public TagRender TagRender { get; internal set; }

        /// <summary>
        /// Tag context 
        /// </summary>
        public Window SettingsWindow { get; internal set; }

        /// <summary>
        /// Shows or hides settings window that is associated with the tag
        /// </summary>
        public DelegateCommand<object> ToggleSettingsCommand { get; internal set; }
    }
}
