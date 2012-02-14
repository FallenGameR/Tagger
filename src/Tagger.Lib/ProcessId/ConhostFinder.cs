using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Tagger.WinAPI;
using Utils.Diagnostics;
using Utils.Extensions;

namespace Tagger
{
    /// <summary>
    /// Finds conhost.exe that hosts a console application 
    /// </summary>
    /// <remarks>
    /// Starting Windows 7 console applications are hosted in a separate process 
    /// that is responsible for rendering console application terminal window. 
    /// See http://blogs.technet.com/b/askperf/archive/2009/10/05/windows-7-windows-server-2008-r2-console-host.aspx
    /// </remarks>
    public sealed class ConhostFinder : IDisposable
    {
        /// <summary>
        /// Native safe handle for WCT session that is used to get conhost
        /// </summary>
        private WctHandle wctSessionHandle;

        /// <summary>
        /// Initializes ConsoleFinder by opening valid WCT session
        /// </summary>
        public ConhostFinder()
        {
            this.wctSessionHandle = NativeAPI.OpenThreadWaitChainSession(0, IntPtr.Zero);

            if (this.wctSessionHandle.IsInvalid)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        /// <summary>
        /// Cleanup native WCt session handle on dispose
        /// </summary>
        public void Dispose()
        {
            this.wctSessionHandle.Dispose();
        }

        /// <summary>
        /// Find process ID of conhost process that hosts a console application
        /// </summary>
        /// <param name="consoleAppProcessId">Process ID of a console application</param>
        /// <returns>
        /// Process ID of conhost process that hosts the console application.
        /// ConsoleAppProcessId is returned if no conhost process if found.
        /// </returns>
        public int GetConhostProcessId(int consoleAppProcessId)
        {
            var query =
                from thread in Process.GetProcessById(consoleAppProcessId).Threads.Cast<ProcessThread>()
                from node in GetThreadWaitChain(thread.Id)
                where node.ObjectType == NativeAPI.WCT_OBJECT_TYPE.Thread
                where Process.GetProcessById(node.ProcessId).ProcessName == "conhost"
                select node.ProcessId;

            var found = query.SingleOrDefault();

            if (found == default(int))
            {
                return consoleAppProcessId;
            }
            else
            {
                return found;
            }
        }

        /// <summary>
        /// Get process wait chain for a particular thread
        /// </summary>
        /// <param name="threadId">Systemwide thread ID</param>
        /// <returns>WCT nodes for processes that this process is waits upon</returns>
        private IEnumerable<NativeAPI.WAITCHAIN_NODE_INFO> GetThreadWaitChain(int threadId)
        {
            var nodes = new NativeAPI.WAITCHAIN_NODE_INFO[NativeAPI.WCT_MAX_NODE_COUNT];
            int length = NativeAPI.WCT_MAX_NODE_COUNT;
            int isCycle;

            var success = NativeAPI.GetThreadWaitChain(
                this.wctSessionHandle,
                IntPtr.Zero,
                NativeAPI.WCT_FLAGS.Process,
                threadId,
                ref length,
                nodes,
                out isCycle);

            if (!success)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return nodes.Take(length);
        }
    }
}
