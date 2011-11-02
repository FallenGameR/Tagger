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
    internal class SafeWaitChainHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeWaitChainHandle ( )
            : base ( true )
        {
        }
        protected override bool ReleaseHandle ( )
        {
            NativeMethods.CloseThreadWaitChainSession ( this.handle );
            return ( true );
        }
    }
}
