using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;

namespace Tagger.WinAPI.Hotkeys
{
    /// <summary>
    /// Registration for global hotkey events
    /// </summary>
    /// <remarks>
    /// Original http://www.liensberger.it/web/blog/?p=207 "Installing a global hot key with C#"
    /// </remarks>    
    public class GlobalHotkey : IDisposable
    {
        /// <summary>
        /// Changes the hotkey behavior so that the keyboard auto-repeat does not yield multiple hotkey notifications
        /// </summary>
        /// <remarks>
        /// This flag works only starting Windows 7
        /// </remarks>
        private const uint NoRepeat = 0x4000;

        private HotkeyReceiverWindow m_ReceiverWindow;

        /// <summary>
        /// Registers global hotkey 
        /// </summary>
        /// <param name="modifier">Modifier keys for registered hotkey</param>
        /// <param name="key">Key for the registered hotkey</param>
        /// <param name="hotkeyPressed">Invoked delegate on hotkey pressed</param>
        public GlobalHotkey(ModifierKeys modifier, Keys key, EventHandler<HotkeyPressedEventArgs> hotkeyPressed)
        {
            m_ReceiverWindow = new HotkeyReceiverWindow();
            m_ReceiverWindow.KeyPressed += (sender, args) => hotkeyPressed(this, args);

            var noRepeatModifier = (uint)modifier | NoRepeat;
            var success = NativeAPI.RegisterHotKey(m_ReceiverWindow.Handle, 0, noRepeatModifier, (uint)key);

            if (!success)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public void Dispose()
        {
            NativeAPI.UnregisterHotKey(m_ReceiverWindow.Handle, 0);            
            m_ReceiverWindow.Dispose();
        }
    }
}