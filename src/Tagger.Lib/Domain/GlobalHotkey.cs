using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;
using Tagger.WinAPI;

namespace Tagger
{
    /// <summary>
    /// Global hotkey registration
    /// </summary>
    /// <remarks>
    /// See also http://www.liensberger.it/web/blog/?p=207 "Installing a global hot key with C#"
    /// </remarks>    
    public sealed class GlobalHotkey : NativeWindow, IDisposable
    {
        /// <summary>
        /// Creates invisible receiver window
        /// </summary>
        public GlobalHotkey(ModifierKeys modifier, Keys key)
        {
            this.CreateHandle(new CreateParams());

            var noRepeatModifier = (uint)modifier | NativeAPI.NoRepeat;
            var success = NativeAPI.RegisterHotKey(this.Handle, 0, noRepeatModifier, (uint)key);

            if (!success)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        /// <summary>
        /// Cleaning up created handle
        /// </summary>
        public void Dispose()
        {
            NativeAPI.UnregisterHotKey(this.Handle, 0);            
            this.DestroyHandle();
        }

        private Counter counter = new Counter("Wndproc");

        /// <summary>
        /// Window procedure used to get hot key event
        /// </summary>
        /// <param name="message">Message received</param>
        protected override void WndProc(ref Message message)
        {
            counter.Next();

            // Default event processing
            base.WndProc(ref message);

            // Filtering out irrelevent events
            if (message.Msg != NativeAPI.WM_HOTKEY) { return; }

            // Do not process anything if there are no subscribers
            if (this.KeyPressed == null) { return; }

            // Fire event with converted event arguments
            this.KeyPressed(this, new HotkeyEventArgs((int)message.LParam));
        }

        /// <summary>
        /// Event that is used to subscribe to global hotkey pressed event
        /// </summary>
        public event EventHandler<HotkeyEventArgs> KeyPressed;
    }
}
