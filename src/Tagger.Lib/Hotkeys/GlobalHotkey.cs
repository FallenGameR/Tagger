using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.ComponentModel;

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
        public GlobalHotkey()
        {
            this.NextHotkeyId = 0;
            this.ReceiverWindow = new HotkeyReceiverWindow();

            this.ReceiverWindow.KeyPressed += (sender, args) =>
            {
                if (this.HotkeyPressed != null)
                {
                    this.HotkeyPressed(this, args);
                }
            };
        }

        public void Dispose()
        {
            for (int i = 0; i < this.NextHotkeyId; i++)
            {
                NativeAPI.UnregisterHotKey(this.ReceiverWindow.Handle, i);
            }

            this.ReceiverWindow.Dispose();
        }

        /// <summary>
        /// Register global hotkey 
        /// </summary>
        /// <param name="modifier">Modifier keys for registered hotkey</param>
        /// <param name="key">Key for the registered hotkey</param>
        public void RegisterHotKey(ModifierKeys modifier, Keys key)
        {
            var success = NativeAPI.RegisterHotKey(this.ReceiverWindow.Handle, this.NextHotkeyId, (uint)modifier, (uint)key);

            if (!success)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            this.NextHotkeyId++;
        }

        /// <summary>
        /// Event that fires when a hotkey was pressed 
        /// </summary>
        public event EventHandler<HotkeyPressedEventArgs> HotkeyPressed;

        private HotkeyReceiverWindow ReceiverWindow { get; set; }
        private int NextHotkeyId { get; set; }
    }
}

/*
public partial class Form1 : Form
{
    KeyboardHook hook = new KeyboardHook();

    public Form1()
    {
        InitializeComponent();

        // register the event that is fired after the key press.
        hook.KeyPressed += hook_KeyPressed;
        // register the control + alt + F12 combination as hot key.
        hook.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Alt, Keys.F12);
    }

    void hook_KeyPressed(object sender, KeyPressedEventArgs e)
    {
        // show the keys pressed in a label.
        label1.Text = e.Modifier.ToString() + ” + “ + e.Key.ToString();
    }
}
/**/
