using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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

            var success = NativeAPI.RegisterHotKey(m_ReceiverWindow.Handle, 0, (uint)modifier, (uint)key);
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