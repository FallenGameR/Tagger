/*------------------------------------------------------------------------------
 * MSDN Magazine Bugslayer Column
 * Copyright © 2007 John Robbins -- All rights reserved.
 -----------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32.SafeHandles;
using WinAPI;

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
            WaitChainTraversal.CloseThreadWaitChainSession ( this.handle );
            return ( true );
        }
    }
}
