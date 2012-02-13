using System;
using System.Windows.Forms;
using System.Windows.Input;

namespace Tagger
{
    /// <summary>
    /// Event arguments for global hotkey press event
    /// </summary>
    public class HotkeyEventArgs : EventArgs
    {
        /// <summary>
        /// Constructs readable arguments for global hotkey event
        /// </summary>
        /// <param name="wmHotkeyLParam">LParam value passed as argument to wm_Hotkey message</param>
        public HotkeyEventArgs(int wmHotkeyLParam)
        {
            this.Key = (Keys)((wmHotkeyLParam >> 16) & 0xFFFF);
            this.Modifier = (ModifierKeys)(wmHotkeyLParam & 0xFFFF);
        }

        /// <summary>
        /// Key pressed
        /// </summary>
        public Keys Key { get; private set; }

        /// <summary>
        /// Key modifiers pressed
        /// </summary>
        public ModifierKeys Modifier { get; private set; }
    }
}
