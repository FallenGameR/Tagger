using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Tagger.WinAPI.WaitChainTraversal
{
    public class ProcessFinder: IDisposable
    {
        private Handle m_Handle;

        public ProcessFinder()
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

        /// <summary>
        /// Find process ID of window hosting the process
        /// </summary>
        /// <param name="pid">Process ID</param>
        /// <returns>
        /// pid of the window that hosts the process;
        /// for GUI applications that would be the passed pid itself;
        /// for CUI applications that would be pid of a conhost.exe (in Windows 7);
        /// </returns>
        public int FindHostProcess( int pid )
        {
            var query =
                from thread in Process.GetProcessById( pid ).Threads.Cast<ProcessThread>()
                from node in GetThreadWaitChain( thread.Id )
                where node.ObjectType == NativeAPI.WCT_OBJECT_TYPE.Thread
                where Process.GetProcessById( node.ProcessId ).ProcessName == "conhost"
                select node.ProcessId;

            var found = query.FirstOrDefault();
            return found == default( int ) ? pid : found;
        }

        private IEnumerable<NativeAPI.WAITCHAIN_NODE_INFO> GetThreadWaitChain( int threadId )
        {
            var nodes = new NativeAPI.WAITCHAIN_NODE_INFO[ NativeAPI.WCT_MAX_NODE_COUNT ];
            int length = NativeAPI.WCT_MAX_NODE_COUNT;
            int isCycle;

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

            return nodes.Take( length );
        }
    }
}
