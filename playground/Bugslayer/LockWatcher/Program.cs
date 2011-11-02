/*------------------------------------------------------------------------------
 * MSDN Magazine Bugslayer Column
 * Copyright © 2007 John Robbins -- All rights reserved.
 -----------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace LockWatcher
{
    class Program
    {
        // The program return codes.
        enum ReturnCode : int
        {
            Success = 0 ,
            InvalidCommandLine = 1 ,
        }

        static int Main ( string [] args )
        {
            ReturnCode ret = ReturnCode.Success;
            LockWatcherArgParser opts = new LockWatcherArgParser ( );
            Boolean parseRet = opts.Parse ( args );
            if ( true == parseRet )
            {
                // Show the logo if required.
                opts.Logo ( );

                // Time to party....
                Boolean keepLooping = ( opts.UpdateTime > 0 );
                // Create the traversal class that's used for all 
                // iterations.
                using ( WaitChainTraversal wct =
                                                new WaitChainTraversal ( ) )
                {
                    do
                    {
                        // The index in the array of process id's that has
                        // ended so I can remove it if looping.
                        int removeIndex = Int32.MaxValue;

                        for ( int i = 0 ; i < opts.ProcessIds.Count ; i++ )
                        {
                            // Dump the wait chains for this process.
                            Boolean procDead = ShowProcessWaitChains ( wct ,
                                                         opts.ProcessIds [ i ] ,
                                                         opts.AllData );
                            if ( true == procDead )
                            {
                                // If there are multiple ending/invalid 
                                // processes, I'm only tracking the last one. If 
                                // looping with the -t switch, I'll get rid of 
                                // all of them eventually. I want to avoid 
                                // messing with the array while I'm looping.
                                removeIndex = i;
                            }

                        }
                        // Has one of the processes I'm monitoring ended?
                        if ( removeIndex != Int32.MaxValue )
                        {
                            opts.ProcessIds.RemoveAt ( removeIndex );
                            removeIndex = Int32.MaxValue;
                            if ( 0 == opts.ProcessIds.Count )
                            {
                                keepLooping = false;
                            }
                        }

                        // Here's where I'll sleep until the next update time.
                        if ( true == keepLooping )
                        {
                            Thread.Sleep ( opts.UpdateTime * 1000 );
                        }

                    } while ( keepLooping );
                }
            }
            else
            {
                ret = ReturnCode.InvalidCommandLine;
            }
            return ( (int)ret );
        }

        [SuppressMessage ( "Wintellect.PerformanceRules" ,
                           "Wintellect1000:AvoidBoxingAndUnboxingInLoops" )]
        static Boolean ShowProcessWaitChains ( WaitChainTraversal wct ,
                                               Int32 processId ,
                                               Boolean showAllData )
        {
            // Look at all the threads for this process.
            Process proc = null;
            try
            {
                proc = Process.GetProcessById ( processId );
            }
            catch ( ArgumentException )
            {
                Console.WriteLine ( Constants.ProcIdInvalidFmt , processId );
            }

            if ( null != proc )
            {
                Console.WriteLine ( Constants.ProcessDataFmt ,
                            BuildProcessName ( proc.ProcessName ) ,
                            proc.Id );
                for ( int j = 0 ; j < proc.Threads.Count ; j++ )
                {
                    // Get the wait chains for this thread.
                    Int32 currThreadId = proc.Threads [ j ].Id;
                    WaitData data = wct.GetThreadWaitChain ( currThreadId );
                    if ( null != data )
                    {
                        DisplayThreadData ( data , showAllData );
                    }
                    else
                    {
                        // This happens when running without admin rights.
                        Console.WriteLine ( Constants.UnableToGetWaitChainsFmt ,
                                            currThreadId );
                    }
                }
                
            } 
            Console.WriteLine ( );
            return ( proc == null );
        }

        static String BuildProcessName ( String name )
        {
            StringBuilder sb = new StringBuilder ( );
            String file = Path.GetFileNameWithoutExtension ( name ).
                                       ToUpper ( CultureInfo.CurrentUICulture );
            sb.AppendFormat ( Constants.ExeFmt , file );
            return ( sb.ToString ( ) );
        }

        [SuppressMessage ( "Wintellect.PerformanceRules" ,
                           "Wintellect1000:AvoidBoxingAndUnboxingInLoops" )]
        static void DisplayThreadData ( WaitData data , Boolean allData )
        {
            // Save the process id value for the first item as this is the 
            // process that owns the thread. I'll use this to check the 
            // later items for threads from other processes.
            Int32 startingPID = data.Nodes [ 0 ].ProcessId;
            StringBuilder sb = new StringBuilder ( );

            // Report the key deadlocked warning if approriate.
            if ( true == data.IsDeadlock )
            {
                Console.WriteLine ( Constants.DeadlockNotification );
            }
            for ( int i = 0 ; i < data.NodeCount ; i++ )
            {
                NativeMethods.WAITCHAIN_NODE_INFO node = data.Nodes [ i ];
                // Do indenting to make the output easier to read.
                String indent = String.Empty;
                if ( i > 0 )
                {
                    indent = new String ( ' ' , i * 3 );
                }

                sb.Length = 0;
                sb.Append ( indent );

                if ( NativeMethods.WCT_OBJECT_TYPE.Thread == node.ObjectType )
                {
                    Process proc = Process.GetProcessById ( node.ProcessId );
                    String procName = BuildProcessName ( proc.ProcessName );

                    switch ( node.ObjectStatus )
                    {
                        case NativeMethods.WCT_OBJECT_STATUS.PidOnly:
                        case NativeMethods.WCT_OBJECT_STATUS.PidOnlyRpcss:
                            sb.AppendFormat ( Constants.OwningFmt ,
                                              node.ProcessId ,
                                              procName );
                            break;
                        default:
                            // Always put the thread value on.
                            sb.AppendFormat ( Constants.TidFmt ,
                                              node.ThreadId );
                            // Is this a block on a thread from another process?
                            if ( ( i > 0 ) &&
                                 ( startingPID != node.ProcessId ) )
                            {
                                // Yes, so show the PID and name.
                                sb.Append ( ' ' );
                                sb.AppendFormat ( Constants.OwningProcessFmt ,
                                                  node.ProcessId ,
                                                  procName );
                            }

                            if ( true == allData )
                            {
                                sb.AppendFormat ( Constants.AllDataFmt ,
                                                  node.ObjectStatus ,
                                   TimeSpan.FromMilliseconds ( node.WaitTime ) ,
                                                  node.ContextSwitches );
                            }
                            else if ( node.ObjectStatus !=
                                       NativeMethods.WCT_OBJECT_STATUS.Blocked )
                            {
                                sb.AppendFormat ( Constants.ObjectStatusFmt ,
                                                  node.ObjectStatus );
                            }
                            break;
                    }
                }
                else
                {
                    switch ( node.ObjectType )
                    {
                        case NativeMethods.WCT_OBJECT_TYPE.CriticalSection:
                        case NativeMethods.WCT_OBJECT_TYPE.SendMessage:
                        case NativeMethods.WCT_OBJECT_TYPE.Mutex:
                        case NativeMethods.WCT_OBJECT_TYPE.Alpc:
                        case NativeMethods.WCT_OBJECT_TYPE.COM:
                        case NativeMethods.WCT_OBJECT_TYPE.ThreadWait:
                        case NativeMethods.WCT_OBJECT_TYPE.ProcessWait:
                        case NativeMethods.WCT_OBJECT_TYPE.COMActivation:
                        case NativeMethods.WCT_OBJECT_TYPE.Unknown:
                            {
                                sb.AppendFormat ( Constants.ObjectTypeFmt ,
                                                  node.ObjectType ,
                                                  node.ObjectStatus );
                                String name = node.ObjectName ( );
                                if ( false == String.IsNullOrEmpty ( name ) )
                                {
                                    sb.AppendFormat ( Constants.ObjectNameFmt ,
                                                      name );
                                }
                            }
                            break;
                        default:
                            Debug.Assert ( false , "Unknown Object Type!" );
                            sb.Append ( Constants.UnknownObjectType );
                            break;
                    }
                }
                Console.WriteLine ( sb.ToString ( ) );
            }
        }
    }
}
