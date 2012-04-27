//-----------------------------------------------------------------------
// <copyright file="WaitChainTraversal.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Tagger.WinAPI
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Wraps all the native Wait Chain Traversal code
    /// </summary>
    /// <remarks>
    /// Original from http://msdn.microsoft.com/en-us/magazine/cc163395.aspx
    /// </remarks>
    public static partial class NativeMethods
    {
        /// <summary>
        /// Max number of nodes in the wait chain 
        /// </summary>
        public const int WCT_MAX_NODE_COUNT = 16;

        /// <summary>
        /// Max length of a named object in chars
        /// </summary>
        public const int WCT_OBJNAME_LENGTH = 128;

        #region WCT_OBJECT_TYPE enum

        /// <summary>
        /// From c:\program files (x86)\microsoft sdks\windows\v7.0a\include\wct.h
        /// </summary>
        public enum WCT_OBJECT_TYPE
        {
            CriticalSection = 1,
            SendMessage,
            Mutex,
            Alpc,
            Com,
            ThreadWait,
            ProcessWait,
            Thread,
            ComActivation,
            Unknown,
            SocketIo,
            SmbIo,
            Max
        }
        
        #endregion

        #region WCT_OBJECT_STATUS enum

        /// <summary>
        /// From c:\program files (x86)\microsoft sdks\windows\v7.0a\include\wct.h
        /// </summary>
        public enum WCT_OBJECT_STATUS
        {
            NoAccess = 1,            // ACCESS_DENIED for this object
            Running,                 // Thread status
            Blocked,                 // Thread status
            PidOnly,                 // Thread status
            PidOnlyRpcss,            // Thread status
            Owned,                   // Dispatcher object status
            NotOwned,                // Dispatcher object status
            Abandoned,               // Dispatcher object status
            Unknown,                 // All objects
            Error,                   // All objects
            Max
        }

        #endregion

        #region WCT_FLAGS enum

        /// <summary>
        /// From c:\program files (x86)\microsoft sdks\windows\v7.0a\include\wct.h
        /// </summary>
        [Flags]
        public enum WCT_FLAGS
        {
            Process = 0x1,
            COM = 0x2,
            CriticalSection = 0x4,
            NetworkIo = 0x8,
            All = Process | COM | CriticalSection | NetworkIo
        }

        #endregion

        /// <summary>
        /// Creates a new WCT session.
        /// </summary>
        /// <param name="flags">The session type. 0 - A synchronous session.</param>
        /// <param name="callback">If the session is asynchronous, this parameter can be a pointer to a WaitChainCallback callback function.</param>
        /// <returns>
        /// If the function succeeds, the return value is a handle to the newly created session.
        /// If the function fails, the return value is NULL. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern WctHandle OpenThreadWaitChainSession(int flags, IntPtr callback);

        /// <summary>
        /// Retrieves the wait chain for the specified thread.
        /// </summary>
        /// <param name="wctHandle">A handle to the WCT session created by the OpenThreadWaitChainSession function.</param>
        /// <param name="context">A pointer to an application-defined context structure to be passed to the callback function for an asynchronous session.</param>
        /// <param name="flags">The wait chain retrieval options.</param>
        /// <param name="threadId">The identifier of the thread.</param>
        /// <param name="nodeCount">
        /// On input, a number from 1 to WCT_MAX_NODE_COUNT that specifies the number of nodes in the 
        /// wait chain. On return, the number of nodes retrieved. If the array cannot contain all the 
        /// nodes of the wait chain, the function fails, GetLastError returns ERROR_MORE_DATA, and 
        /// this parameter receives the number of array elements required to contain all the nodes.
        /// </param>
        /// <param name="nodeInfoArray">An array of WAITCHAIN_NODE_INFO structures that receives the wait chain.</param>
        /// <param name="isCycle">If the function detects a deadlock, this variable is set to TRUE; otherwise, it is set to FALSE.</param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero. To retrieve extended error information, call GetLastError.
        /// </returns>
        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool GetThreadWaitChain(WctHandle wctHandle, IntPtr context, WCT_FLAGS flags, int threadId, ref int nodeCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] [In, Out] WAITCHAIN_NODE_INFO[] nodeInfoArray, out int isCycle);

        /// <summary>
        /// Closes the specified WCT session and cancels any outstanding asynchronous operations.
        /// </summary>
        /// <param name="wctHandle">A handle to the WCT session created by the OpenThreadWaitChainSession function.</param>
        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern void CloseThreadWaitChainSession(IntPtr wctHandle);

        #region WAITCHAIN_NODE_INFO struct

        /// <summary>
        /// The data structure returned indicating blocked threads
        /// </summary>
        /// <remarks>
        /// Even though the ObjectName field is a character array, it's declared
        /// as a ushort because of a bug in the CLR marshalling with character
        /// arrays and the fixed keyword. By using the ushort, the structure is
        /// now blittable, meaning the managed and native types are the same.
        /// A char is not blittable because it has multiple representations in 
        /// native code (ANSI and UNICODE). 
        /// <para/>
        /// Fortunately, to get the actual character array in ObjectName, you 
        /// can cast the ushort pointer to a char pointer passed to the String
        /// constructor.
        /// <para/>
        /// http://msdn.microsoft.com/en-us/site/ms681422
        /// </remarks>
        [StructLayout(LayoutKind.Explicit, Size = 280)]
        public unsafe struct WAITCHAIN_NODE_INFO
        {
            [FieldOffset(0x0)]
            public WCT_OBJECT_TYPE ObjectType;
            [FieldOffset(0x4)]
            public WCT_OBJECT_STATUS ObjectStatus;

            // The name union
            [FieldOffset(0x8)]
            private fixed ushort RealObjectName[WCT_OBJNAME_LENGTH];
            [FieldOffset(0x108)]
            public int TimeOutLowPart;
            [FieldOffset(0x10C)]
            public int TimeOutHiPart;
            [FieldOffset(0x110)]
            public int Alertable;

            // The thread union
            [FieldOffset(0x8)]
            public int ProcessId;
            [FieldOffset(0xC)]
            public int ThreadId;
            [FieldOffset(0x10)]
            public int WaitTime;
            [FieldOffset(0x14)]
            public int ContextSwitches;

            /// <summary>
            /// Type casting to get the ObjectName field
            /// </summary>
            public string ObjectName
            {
                get
                {
                    fixed (WAITCHAIN_NODE_INFO* p = &this)
                    {
                        return (p->RealObjectName[0] != '\0')
                            ? new string((char*)p->RealObjectName)
                            : string.Empty;
                    }
                }
            }
        }

        #endregion
    }
}
