/*------------------------------------------------------------------------------
 * MSDN Magazine Bugslayer Column
 * Copyright © 2007 John Robbins -- All rights reserved.
 -----------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using WinAPI;

namespace LockWatcher
{
    internal sealed class WaitData
    {
        private WaitChainTraversal.WAITCHAIN_NODE_INFO[] data;
        private Boolean isDeadlock;
        private Int32 nodeCount;

        public WaitData( WaitChainTraversal.WAITCHAIN_NODE_INFO[] data,
                          Int32 nodeCount,
                          Boolean isDeadlock )
        {
            this.data = data;
            this.nodeCount = nodeCount;
            this.isDeadlock = isDeadlock;
        }

        public WaitChainTraversal.WAITCHAIN_NODE_INFO[] Nodes
        {
            get { return (data); }
        }

        public Int32 NodeCount
        {
            get { return (nodeCount); }
        }

        public Boolean IsDeadlock
        {
            get { return (isDeadlock); }
        }
    }

    internal sealed class WaitChainTraversalObj: IDisposable
    {
        private SafeWaitChainHandle waitChainHandle;

        public WaitChainTraversalObj()
        {
            waitChainHandle = WaitChainTraversal.OpenThreadWaitChainSession( 0, IntPtr.Zero );
            if( waitChainHandle.IsInvalid )
            {
                throw new InvalidOperationException( Constants.NoOpenWCTHandle );
            }
        }

        public WaitData GetThreadWaitChain( Int32 threadId )
        {
            WaitChainTraversal.WAITCHAIN_NODE_INFO[] data = new WaitChainTraversal.WAITCHAIN_NODE_INFO[ WaitChainTraversal.WCT_MAX_NODE_COUNT ];
            uint isDeadlock = 0;
            uint nodeCount = WaitChainTraversal.WCT_MAX_NODE_COUNT;
            Boolean ret = WaitChainTraversal.GetThreadWaitChain(
                waitChainHandle,
                IntPtr.Zero,
                WaitChainTraversal.WCT_FLAGS.All,
                threadId,
                ref nodeCount,
                data,
                out isDeadlock );

            WaitData retData = null;
            if( true == ret )
            {
                retData = new WaitData( data,
                                         (Int32) nodeCount,
                                         1 == isDeadlock );
            }

            return (retData);

        }


        public void Dispose()
        {
            waitChainHandle.Dispose();
        }
    }
}
