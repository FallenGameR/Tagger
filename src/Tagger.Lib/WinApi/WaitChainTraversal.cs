using System;
using System.Runtime.InteropServices;
using Tagger.WinAPI.WaitChainTraversal;

namespace Tagger.WinAPI
{
    /// <summary>
    /// Wraps all the native Wait Chain Traversal code
    /// </summary>
    /// <remarks>
    /// Original from http://msdn.microsoft.com/en-us/magazine/cc163395.aspx
    /// </remarks>
    public static partial class NativeAPI
    {
        /// <summary>
        /// Max number of nodes in the wait chain 
        /// </summary>
        public const int WCT_MAX_NODE_COUNT = 16;

        /// <summary>
        /// Max length of a named object in chars
        /// </summary>
        public const int WCT_OBJNAME_LENGTH = 128;

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
        /// 
        /// Fortunately, to get the actual character array in ObjectName, you 
        /// can cast the ushort pointer to a char pointer passed to the String
        /// constructor.
        /// 
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
        };
        
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
        };

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

        [DllImport("advapi32.dll", SetLastError = true)]
        public extern static bool GetThreadWaitChain(Handle wctHandle, IntPtr context, WCT_FLAGS flags, Int32 threadId, ref int nodeCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] [In, Out] WAITCHAIN_NODE_INFO[] nodeInfoArray, out int isCycle);

        [DllImport("advapi32.dll", SetLastError = true)]
        public extern static Handle OpenThreadWaitChainSession(int flags, IntPtr callback);

        [DllImport("advapi32.dll", SetLastError = true)]
        public extern static void CloseThreadWaitChainSession(IntPtr wctHandle);
    }
}
