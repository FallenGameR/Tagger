using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using WinAPI.WaitChainTraversal;

namespace LockWatcher
{
    internal sealed class WaitChainTraversal: IDisposable
    {
        private Handle m_Handle;

        public WaitChainTraversal()
        {
            m_Handle = NativeAPI.OpenThreadWaitChainSession( 0, IntPtr.Zero );

            if( m_Handle.IsInvalid )
            {
                throw new Win32Exception( Marshal.GetLastWin32Error() );
            }
        }

        public void Dispose()
        {
            m_Handle.Dispose();
        }

        public IEnumerable<NativeAPI.WAITCHAIN_NODE_INFO> GetThreadWaitChain( Int32 threadId )
        {
            var data = new NativeAPI.WAITCHAIN_NODE_INFO[ NativeAPI.WCT_MAX_NODE_COUNT ];
            uint nodeCount = NativeAPI.WCT_MAX_NODE_COUNT;
            uint isCycle = 0;

            var success = NativeAPI.GetThreadWaitChain(
                m_Handle,
                IntPtr.Zero,
                NativeAPI.WCT_FLAGS.CriticalSection,
                threadId,
                ref nodeCount,
                data,
                out isCycle );
            if( !success )
            {
                throw new Win32Exception( Marshal.GetLastWin32Error() );
            }

            return data.Take( (int) nodeCount );
        }
    }
}
