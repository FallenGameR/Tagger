/*------------------------------------------------------------------------------
 * MSDN Magazine Bugslayer Column
 * Copyright © 2007 John Robbins -- All rights reserved.
 -----------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace LockWatcher
{
    class SafeModuleHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeModuleHandle ( ) 
            : base ( true ) 
        { 
        }

        protected override Boolean ReleaseHandle ( )
        {
            return ( NativeMethods.FreeLibrary ( this.handle ) );
        }
    }
}
