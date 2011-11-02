/*------------------------------------------------------------------------------
 * MSDN Magazine Bugslayer Column
 * Copyright © 2007 John Robbins -- All rights reserved.
 -----------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace LockWatcher
{
    internal sealed class WaitData
    {
        private NativeMethods.WAITCHAIN_NODE_INFO [] data;
        private Boolean isDeadlock;
        private Int32 nodeCount;

        public WaitData ( NativeMethods.WAITCHAIN_NODE_INFO [] data ,
                          Int32 nodeCount ,
                          Boolean isDeadlock )
        {
            this.data = data;
            this.nodeCount = nodeCount;
            this.isDeadlock = isDeadlock;
        }

        public NativeMethods.WAITCHAIN_NODE_INFO [] Nodes
        {
            get { return ( data ); }
        }

        public Int32 NodeCount
        {
            get { return ( nodeCount ); }
        }

        public Boolean IsDeadlock
        {
            get { return ( isDeadlock ); }
        }
    }

    internal sealed class WaitChainTraversal : IDisposable
    {
        private SafeWaitChainHandle waitChainHandle;

        public WaitChainTraversal ( )
        {
            waitChainHandle = NativeMethods.OpenThreadWaitChainSession ( );
        }

        public WaitData GetThreadWaitChain ( Int32 threadId )
        {
            NativeMethods.WAITCHAIN_NODE_INFO [] data = new NativeMethods.
                      WAITCHAIN_NODE_INFO [ NativeMethods.WCT_MAX_NODE_COUNT ];
            uint isDeadlock = 0;
            uint nodeCount = NativeMethods.WCT_MAX_NODE_COUNT;
            Boolean ret = NativeMethods.GetThreadWaitChain ( waitChainHandle ,
                                                             threadId ,
                                                             ref nodeCount ,
                                                             data ,
                                                             out isDeadlock );
            WaitData retData = null;
            if ( true == ret )
            {
                retData = new WaitData ( data , 
                                         (Int32)nodeCount , 
                                         1 == isDeadlock );
            }

            return ( retData );

        }


        public void Dispose ( )
        {
            waitChainHandle.Dispose ( );
        }
    }
}
