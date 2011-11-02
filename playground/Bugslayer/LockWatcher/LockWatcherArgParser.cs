/*------------------------------------------------------------------------------
 * MSDN Magazine Bugslayer Column
 * Copyright © 2007 John Robbins -- All rights reserved.
 -----------------------------------------------------------------------------*/
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace LockWatcher
{
    /// <summary>
    /// A simple class to take care of the LockWatcher command line arguments.
    /// </summary>
    internal class LockWatcherArgParser : ArgParser
    {
        #region Option Properties

        // Just used internally by the Logo method.
        private Boolean noLogo;

        private Boolean allData;
        /// <summary>
        /// Show all the thread data.
        /// </summary>
        public Boolean AllData
        {
            [DebuggerStepThrough]
            get { return allData; }
            [DebuggerStepThrough]
            set { allData = value; }
        }

        private Int32 updateTime;
        /// <summary>
        /// The amount of time to wait between each update.
        /// </summary>
        public Int32 UpdateTime
        {
            [DebuggerStepThrough]
            get { return updateTime; }
        }

        private List<int> processIds;
        /// <summary>
        /// Returns the list of process IDs to process.
        /// </summary>
        public List<int> ProcessIds
        {
            [DebuggerStepThrough]
            get { return processIds; }
            [DebuggerStepThrough]
            set { processIds = value; }
        }

        #endregion

        // The error message to display on errors.
        private String errorMessage;

        public LockWatcherArgParser ( )
            : base ( new String [] { "?" , "h" , "help" ,
                                     "a" , "all" ,
                                     "nologo" } ,
                     new String [] { "t" , "time" } )
        {
            errorMessage = String.Empty;

            // Set all the defaults.
            ProcessIds = new List<int> ( );
        }

        protected override SwitchStatus OnSwitch ( string switchSymbol ,
                                                   string switchValue )
        {
            SwitchStatus ss = SwitchStatus.NoError;
            String errorMsg = null;
            switch ( switchSymbol )
            {
                case "?":
                case "help":
                case "h":
                    ss = SwitchStatus.ShowUsage;
                    break;

                case "a":
                case "all":
                    AllData = true;
                    break;

                case "nologo":
                    noLogo = true;
                    break;

                case "t":
                case "time":
                    if ( false == Int32.TryParse ( switchValue ,
                                                   out updateTime ) )
                    {
                        ss = SwitchStatus.Error;
                        errorMsg = Constants.CannotParseTime;
                    }
                    break;
                /////////////////////////////////
                // Everything else is an error //
                /////////////////////////////////
                default:
                    ss = SwitchStatus.Error;
                    break;
            }
            if ( ss == SwitchStatus.Error )
            {
                errorMessage = errorMsg;
            }
            return ( ss );
        }

        protected override SwitchStatus OnNonSwitch ( string value )
        {
            SwitchStatus ss = SwitchStatus.NoError;
            Int32 processId;
            // It's a process ID so try and add it to the list.
            if ( false == Int32.TryParse ( value , out processId ) )
            {
                ss = SwitchStatus.Error;
                errorMessage = Constants.CannotParseProcessId;
            }
            else
            {
                ProcessIds.Add ( processId );
            }
            return ( ss );
        }

        protected override SwitchStatus OnDoneParse ( )
        {
            // If no process ids were specified, I'll add all of them here.
            if ( ProcessIds.Count == 0 )
            {
                Process [] procs = Process.GetProcesses ( );
                for ( int i = 0 ; i < procs.Length ; i++ )
                {
                    // Add all but the System and System Idle Processes.
                    if ( ( 0 != procs[i].Id ) && ( 4 != procs[i].Id ) )
                    {
                        ProcessIds.Add ( procs [ i ].Id );
                    }
                }
            }
            return ( SwitchStatus.NoError );
        }

        public void Logo ( )
        {
            if ( false == noLogo )
            {
                // Get the EXE module.
                ProcessModule exe = Process.GetCurrentProcess ( ).Modules [ 0 ];
                Console.WriteLine ( Constants.LogoMessage ,
                                    exe.FileVersionInfo.FileVersion );
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String,System.Object,System.Object)")]
        public override void OnUsage ( string errorInfo )
        {
            Logo ( );
            Console.WriteLine ( Constants.UsageMessage );
            if ( null != errorInfo )
            {
                Console.WriteLine ( Constants.ParseError , errorInfo );
            }
            if ( false == String.IsNullOrEmpty ( errorMessage ) )
            {
                Console.WriteLine ( "{0}: {1}" ,
                                    Constants.ErrorText ,
                                    errorMessage );
            }
        }
    }
}
