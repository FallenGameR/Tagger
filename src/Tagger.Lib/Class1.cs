using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Tagger.Lib
{
    public static class WinApi
    {
        public enum EventObject : uint
        {
            LocationChange = 0x800B
        }

        public delegate void WinEventDelegate( 
            IntPtr hWinEventHook, 
            uint eventType, 
            IntPtr hwnd, 
            int idObject, 
            int idChild, 
            uint dwEventThread, 
            uint dwmsEventTime );

        [DllImport( "user32.dll" )]
        public static extern IntPtr SetWinEventHook( 
            uint eventMin, 
            uint eventMax, 
            IntPtr hmodWinEventProc, 
            WinEventDelegate lpfnWinEventProc, 
            uint idProcess,
            uint idThread, 
            uint dwFlags );

        [DllImport( "user32.dll" )]
        static extern bool UnhookWinEvent( IntPtr hWinEventHook );
    }
}
