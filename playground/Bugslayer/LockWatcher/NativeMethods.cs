/*------------------------------------------------------------------------------
 * MSDN Magazine Bugslayer Column
 * Copyright © 2007 John Robbins -- All rights reserved.
 -----------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace LockWatcher
{
    /// <summary>
    /// Wraps all the native Wait Chain Traversal code.
    /// </summary>
    internal static partial class NativeMethods
    {
        // Max. number of nodes in the wait chain
        internal const int WCT_MAX_NODE_COUNT = 16;
        // Max. length of a named object.
        internal const int WCT_OBJNAME_LENGTH = 128;

        /// <summary>
        /// The data structure returned indicating blocked threads.
        /// </summary>
        /// <remarks>
        /// Even though the ObjectName field is a character array, it's declared
        /// as a ushort because of a bug in the CLR marshalling with character
        /// arrays and the fixed keyword. By using the ushort, the structure is
        /// now blittable, meaning the managed and native types are the same.
        /// A char is not blittable because it has multiple representations in 
        /// native code (ANSI and UNICODE). 
        /// Fortunately, to get the actual character array in ObjectName, you 
        /// can cast the ushort pointer to a char pointer passed to the String
        /// constructor.
        /// </remarks>
        [StructLayout ( LayoutKind.Explicit , Size = 280 )]
        internal unsafe struct WAITCHAIN_NODE_INFO
        {
            [FieldOffset ( 0x0 )]
            public WCT_OBJECT_TYPE ObjectType;
            [FieldOffset ( 0x4 )]
            public WCT_OBJECT_STATUS ObjectStatus;
            // The name union.
            [FieldOffset ( 0x8 )]
            private fixed ushort RealObjectName [ WCT_OBJNAME_LENGTH ];
            [FieldOffset ( 0x108 )]
            public Int32 TimeOutLowPart;
            [FieldOffset ( 0x10C )]
            public Int32 TimeOutHiPart;
            [FieldOffset ( 0x110 )]
            public Int32 Alertable;

            // The thread union.
            [FieldOffset ( 0x8 )]
            public Int32 ProcessId;
            [FieldOffset ( 0xC )]
            public Int32 ThreadId;
            [FieldOffset ( 0x10 )]
            public Int32 WaitTime;
            [FieldOffset ( 0x14 )]
            public Int32 ContextSwitches;

            // Does the work to get the ObjectName field.
            public String ObjectName ( )
            {
                fixed ( WAITCHAIN_NODE_INFO* p = &this )
                {
                    return ( p->RealObjectName [ 0 ] != '\0' ) 
                             ? new String ( (char*)p->RealObjectName ) 
                             : String.Empty ;
                }
            }
        }

        internal enum WCT_OBJECT_TYPE
        {
            CriticalSection = 1 ,
            SendMessage ,
            Mutex ,
            Alpc ,
            COM ,
            ThreadWait ,
            ProcessWait ,
            Thread ,
            COMActivation ,
            Unknown ,
        } ;

        internal enum WCT_OBJECT_STATUS
        {
            NoAccess = 1 ,            // ACCESS_DENIED for this object
            Running ,                 // Thread status
            Blocked ,                 // Thread status
            PidOnly ,                 // Thread status
            PidOnlyRpcss ,            // Thread status
            Owned ,                   // Dispatcher object status
            NotOwned ,                // Dispatcher object status
            Abandoned ,               // Dispatcher object status
            Unknown ,                 // All objects
            Error ,                   // All objects
        } ;

        [Flags]
        internal enum WCT_FLAGS
        {
            Flag = 0x1 ,
            COM = 0x2 ,
            Proc = 0x4 ,
            All = Flag | COM | Proc
        }

        // Allow the code to compile on XP/Server 2003.
        [SuppressMessage ( "Microsoft.Interoperability" ,
                           "CA1400:PInvokeEntryPointsShouldExist" )]
        [DllImport ( "ADVAPI32.DLL" ,
                     SetLastError = true ,
                     ExactSpelling = true ,
                     CharSet = CharSet.Unicode ,
                     EntryPoint = "CloseThreadWaitChainSession" )]
        extern static private
        void RealCloseThreadWaitChainSession ( IntPtr wctHandle );

        static public void CloseThreadWaitChainSession ( IntPtr handle )
        {
            RealCloseThreadWaitChainSession ( handle );
        }

        // Allow the code to compile on XP/Server 2003.
        [SuppressMessage ( "Microsoft.Interoperability" ,
                           "CA1400:PInvokeEntryPointsShouldExist" )]
        [DllImport ( "ADVAPI32.DLL" ,
                      EntryPoint = "OpenThreadWaitChainSession" )]
        extern static private
        SafeWaitChainHandle RealOpenThreadWaitChainSession ( int flags ,
                                                             IntPtr callback );

        // Keep the module handle around for the life of the application as the
        // WCT code has pointers into it.
        static SafeModuleHandle oleModule;

        static public SafeWaitChainHandle OpenThreadWaitChainSession ( )
        {
            // Get the COM APIs into WCT. I have no idea why WCT just doesn't
            // do this itself.
            if ( null == oleModule )
            {
                oleModule = LoadLibraryW ( "OLE32.DLL" );
                IntPtr coGetCallState = GetProcAddress ( oleModule ,
                                                         "CoGetCallState" );
                IntPtr coGetActivationState = GetProcAddress ( oleModule ,
                                                       "CoGetActivationState" );
                // Register these functions with WCT.
                RegisterWaitChainCOMCallback ( coGetCallState ,
                                               coGetActivationState );
            }


            SafeWaitChainHandle wctHandle = RealOpenThreadWaitChainSession ( 0 ,
                                                                  IntPtr.Zero );
            if ( true == wctHandle.IsInvalid )
            {
                throw new InvalidOperationException (
                                                    Constants.NoOpenWCTHandle );
            }
            return ( wctHandle );
        }

        // Allow the code to compile on XP/Server 2003.
        [SuppressMessage ( "Microsoft.Interoperability" ,
                           "CA1400:PInvokeEntryPointsShouldExist" )]
        [DllImport ( "ADVAPI32.DLL" ,
                     ExactSpelling = true ,
                     SetLastError = true )]
        internal static extern
            void RegisterWaitChainCOMCallback ( IntPtr callStateCallback ,
                                               IntPtr activationStateCallback );

        // Allow the code to compile on XP/Server 2003.
        [SuppressMessage ( "Microsoft.Interoperability" ,
                           "CA1400:PInvokeEntryPointsShouldExist" )]
        [DllImport ( "ADVAPI32.DLL" , EntryPoint = "GetThreadWaitChain" )]
        [return: MarshalAs ( UnmanagedType.Bool )]
        extern static private
        Boolean RealGetThreadWaitChain ( SafeWaitChainHandle WctHandle ,
                                         IntPtr Context ,
                                         WCT_FLAGS Flags ,
                                         Int32 ThreadId ,
                                         ref uint NodeCount ,
                    [MarshalAs ( UnmanagedType.LPArray , SizeParamIndex = 4 )]
                    [In , Out]
                         WAITCHAIN_NODE_INFO [] NodeInfoArray ,
                                         out uint IsCycle );

        internal static Boolean GetThreadWaitChain (
                                        SafeWaitChainHandle chainHandle ,
                                        Int32 threadId ,
                                        ref uint NodeCount ,
                                        WAITCHAIN_NODE_INFO [] NodeInfoArray ,
                                        out uint IsCycle )
        {
            Boolean ret = RealGetThreadWaitChain ( chainHandle ,
                                                   IntPtr.Zero ,
                                                   WCT_FLAGS.All ,
                                                   threadId ,
                                                   ref NodeCount ,
                                                   NodeInfoArray ,
                                                   out IsCycle );
            return ( ret );
        }

        [DllImport ( "KERNEL32.DLL" ,
                      SetLastError = true ,
                      ExactSpelling = true ,
                      CharSet = CharSet.Unicode )]
        internal static extern
            SafeModuleHandle LoadLibraryW ( String lpFileName );

        [DllImport ( "KERNEL32.DLL" ,
                      SetLastError = true ,
                      ExactSpelling = true )]
        [return: MarshalAs ( UnmanagedType.Bool )]
        internal static extern Boolean FreeLibrary ( IntPtr hModule );


        [DllImport ( "KERNEL32.DLL" ,
                     SetLastError = true ,
                     ExactSpelling = true ,
                     CharSet = CharSet.Ansi )]
        internal static extern
            IntPtr GetProcAddress ( SafeModuleHandle hModule ,
                                    String lpProcName );
    }
}
