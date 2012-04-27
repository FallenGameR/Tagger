//-----------------------------------------------------------------------
// <copyright file="ConhostFinder.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Tagger
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Tagger.WinAPI;

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
        private readonly WctHandle wctSessionHandle;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConhostFinder"/> class by opening a valid WCT session 
        /// </summary>
        public ConhostFinder()
        {
            this.wctSessionHandle = NativeMethods.OpenThreadWaitChainSession(0, IntPtr.Zero);

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
                from node in this.GetThreadWaitChain(thread.Id)
                where node.ObjectType == NativeMethods.WCT_OBJECT_TYPE.Thread
                where Process.GetProcessById(node.ProcessId).ProcessName == "conhost"
                select node.ProcessId;

            var found = query.SingleOrDefault();
            return found == default(int) ? consoleAppProcessId : found;
        }

        /// <summary>
        /// Get process wait chain for a particular thread
        /// </summary>
        /// <param name="threadId">Systemwide thread ID</param>
        /// <returns>WCT nodes for processes that this process is waits upon</returns>
        private IEnumerable<NativeMethods.WAITCHAIN_NODE_INFO> GetThreadWaitChain(int threadId)
        {
            var nodes = new NativeMethods.WAITCHAIN_NODE_INFO[NativeMethods.WCT_MAX_NODE_COUNT];
            var length = NativeMethods.WCT_MAX_NODE_COUNT;
            int isCycle;

            var success = NativeMethods.GetThreadWaitChain(
                this.wctSessionHandle,
                IntPtr.Zero,
                NativeMethods.WCT_FLAGS.Process,
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
