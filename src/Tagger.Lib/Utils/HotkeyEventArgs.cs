﻿//-----------------------------------------------------------------------
// <copyright file="HotkeyEventArgs.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Tagger
{
    using System;
    using System.Windows.Forms;
    using System.Windows.Input;

    /// <summary>
    /// Event arguments for global hotkey press event
    /// </summary>
    public class HotkeyEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HotkeyEventArgs"/> class. 
        /// </summary>
        /// <param name="messageHotkeyParam">
        /// LParam value passed as argument to wm_Hotkey message
        /// </param>
        /// <remarks>
        /// Constructs readable arguments for global hotkey event
        /// </remarks>
        public HotkeyEventArgs(int messageHotkeyParam)
        {
            this.Key = (Keys)((messageHotkeyParam >> 16) & 0xFFFF);
            this.Modifier = (ModifierKeys)(messageHotkeyParam & 0xFFFF);
        }

        /// <summary>
        /// Gets key that was pressed
        /// </summary>
        public Keys Key { get; private set; }

        /// <summary>
        /// Gets key modifiers that were pressed
        /// </summary>
        public ModifierKeys Modifier { get; private set; }
    }
}
