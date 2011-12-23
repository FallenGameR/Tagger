using System;

namespace Tagger.WinAPI.Hotkeys
{
    /// <summary>
    /// The keys that must be pressed in combination with the key specified by 
    /// the uVirtKey parameter in order to generate the WM_HOTKEY message
    /// </summary>
    [Flags]
    public enum ModifierKeys : uint
    {
        /// <summary>
        /// No flags
        /// </summary>
        None = 0x0000,

        /// <summary>
        /// Either ALT key must be held down
        /// </summary>
        Alt = 0x0001,

        /// <summary>
        /// Either CTRL key must be held down
        /// </summary>
        Control = 0x0002,

        /// <summary>
        /// Either SHIFT key must be held down
        /// </summary>
        Shift = 0x0004,

        /// <summary>
        /// Either WINDOWS key was held down
        /// </summary>
        Win = 0x0008,

        /// <summary>
        /// Changes the hotkey behavior so that the keyboard auto-repeat does not yield multiple hotkey notifications
        /// </summary>
        /// <remarks>
        /// This flag works starting Windows 7
        /// </remarks>
        NoRepeat = 0x4000,
    }
}
