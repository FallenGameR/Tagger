using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace WinAPI.WaitChainTraversal
{
    public sealed class WaitChainTraversal: IDisposable
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

        public int FindParentConhost( int tid )
        {
            var data = GetThreadWaitChain( tid );

            foreach( var item in data )
            {
                if( NativeAPI.WCT_OBJECT_TYPE.Thread == item.ObjectType )
                {
                    if( Process.GetProcessById( item.ProcessId ).ProcessName == "conhost" )
                    {
                        return item.ProcessId;
                    }
                }
            }

            return -1;
        }

        private IEnumerable<NativeAPI.WAITCHAIN_NODE_INFO> GetThreadWaitChain( int threadId )
        {
            var nodes = new NativeAPI.WAITCHAIN_NODE_INFO[ NativeAPI.WCT_MAX_NODE_COUNT ];
            uint length = NativeAPI.WCT_MAX_NODE_COUNT;
            uint isCycle;

            var success = NativeAPI.GetThreadWaitChain(
                m_Handle,
                IntPtr.Zero,
                NativeAPI.WCT_FLAGS.Process,
                threadId,
                ref length,
                nodes,
                out isCycle );
            if( !success )
            {
                throw new Win32Exception( Marshal.GetLastWin32Error() );
            }

            return nodes.Take( (int) length );
        }
    }
}
