using System;
using System.Windows.Forms;
using System.Windows.Input;

namespace Tagger.WinAPI.Hotkeys
{
    /// <summary>
    /// Args for event that is fired after hot key is pressed
    /// </summary>
    public class HotkeyPressedEventArgs : EventArgs
    {
        public ModifierKeys Modifier { get; set; }
        public Keys Key { get; set; }
    }
}
