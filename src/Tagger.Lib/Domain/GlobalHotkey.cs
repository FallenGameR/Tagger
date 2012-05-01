//-----------------------------------------------------------------------
// <copyright file="GlobalHotkey.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Tagger
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Windows.Input;
    using Tagger.WinAPI;

    /// <summary>
    /// Global hotkey registration
    /// </summary>
    /// <remarks>
    /// See also http://www.liensberger.it/web/blog/?p=207 "Installing a global hot key with C#"
    /// </remarks>    
    public sealed class GlobalHotkey : NativeWindow, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalHotkey"/> class. Creates invisible receiver window.
        /// </summary>
        /// <param name="modifier">Key modifier to use for global hotkey</param>
        /// <param name="key">Key valuse to use for global hotkey</param>
        [SecurityPermission(SecurityAction.LinkDemand)]
        public GlobalHotkey(ModifierKeys modifier, Key key)
        {
            this.CreateHandle(new CreateParams());

            var virtualKey = KeyInterop.VirtualKeyFromKey(key);
            var noRepeatModifier = (uint)modifier | NativeMethods.NoRepeat;
            var success = NativeMethods.RegisterHotKey(this.Handle, 0, noRepeatModifier, (uint)virtualKey);

            if (!success)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        /// <summary>
        /// Event that is used to subscribe to global hotkey pressed event
        /// </summary>
        public event EventHandler<HotkeyEventArgs> KeyPressed;

        /// <summary>
        /// Cleaning up created handle
        /// </summary>
        [SecurityPermission(SecurityAction.LinkDemand)]
        public void Dispose()
        {
            NativeMethods.UnregisterHotKey(this.Handle, 0);            
            this.DestroyHandle();
        }

        /// <summary>
        /// Window procedure used to get hot key event
        /// </summary>
        /// <param name="message">Message received</param>
        protected override void WndProc(ref Message message)
        {
            // Default event processing
            base.WndProc(ref message);

            // Filtering out irrelevent events
            if (message.Msg != NativeMethods.WM_HOTKEY)
            {
                return;
            }

            // Do not process anything if there are no subscribers
            if (this.KeyPressed == null)
            {
                return;
            }

            // Fire event with converted event arguments
            this.KeyPressed(this, new HotkeyEventArgs((int)message.LParam));
        }
    }
}
