using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using WinAPI;
using System.Linq;

namespace LockWatcher
{
    internal sealed class WaitData
    {
        public WaitData( WaitChainTraversal.WAITCHAIN_NODE_INFO[] data, Int32 nodeCount, Boolean isDeadlock )
        {
            Nodes = data;
            NodeCount = nodeCount;
        }

        public WaitChainTraversal.WAITCHAIN_NODE_INFO[] Nodes { get; private set; }

        public Int32 NodeCount { get; private set; }
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

        public IEnumerable<WaitChainTraversal.WAITCHAIN_NODE_INFO> GetThreadWaitChain( Int32 threadId )
        {
            var data = new WaitChainTraversal.WAITCHAIN_NODE_INFO[ WaitChainTraversal.WCT_MAX_NODE_COUNT ];
            uint isDeadlock = 0;
            uint nodeCount = WaitChainTraversal.WCT_MAX_NODE_COUNT;
            var ret = WaitChainTraversal.GetThreadWaitChain( waitChainHandle, IntPtr.Zero, WaitChainTraversal.WCT_FLAGS.All, threadId, ref nodeCount, data, out isDeadlock );


            return data.Take( (int) nodeCount );
        }


        public void Dispose()
        {
            waitChainHandle.Dispose();
        }
    }
}
