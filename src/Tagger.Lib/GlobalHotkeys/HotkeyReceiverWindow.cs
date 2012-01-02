using System;
using System.Windows.Forms;
using System.Windows.Input;

namespace Tagger.WinAPI.Hotkeys
{
    /// <summary>
    /// Window that is used to receive hotkey events
    /// </summary>
    internal class HotkeyReceiverWindow : NativeWindow, IDisposable
    {
        private const int WM_HOTKEY = 0x0312;

        public HotkeyReceiverWindow()
        {
            CreateHandle(new CreateParams());
        }

        public void Dispose()
        {
            DestroyHandle();
        }

        /// <summary>
        /// Window procedure used to get hot key event
        /// </summary>
        /// <param name="m">Message received</param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg != WM_HOTKEY) { return; }

            var key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
            var modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

            if (KeyPressed != null)
            {
                KeyPressed(this, new HotkeyPressedEventArgs { Key = key, Modifier = modifier });
            }
        }

        public event EventHandler<HotkeyPressedEventArgs> KeyPressed;
    }
}
