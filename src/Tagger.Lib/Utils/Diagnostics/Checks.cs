using System;
using System.Diagnostics;

namespace Utils.Diagnostics
{
    /// <summary>
    /// Design By Contract Checks.
    /// Each method generates an exception (by default) 
    /// or a trace assertion statement if the contract is broken.
    /// </summary>
    /// <example>
    /// Trace.Listeners.Clear();
    /// Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
    ///	Check.Require(x > 1, "x must be > 1");
    /// </example>
    /// <remarks>
    /// ASP.NET 2.0 clients can not use Trace Listeners (only Debug).
    /// For a Release build only exception-handling is possible.
    /// </remarks>
    /// <remarks>
    /// Taken from NHibernate Best Practices (on CodeProject site)
    /// </remarks>
    public static class Check
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        static Check( )
        {
            UseAssertions = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Precondition check - should run regardless of preprocessor directives.
        /// </summary>
        [DebuggerStepThrough]
        public static void Require( bool assertion )
        {
            if( UseExceptions )
            {
                if( !assertion ) throw new DesignByContractException( "Precondition failed." );
            }
            else
            {
                Trace.Assert( assertion, "Precondition failed." );
            }
        }

        /// <summary>
        /// Precondition check - should run regardless of preprocessor directives.
        /// </summary>
        [DebuggerStepThrough]
        public static void Require( bool assertion, string message )
        {
            if( UseExceptions )
            {
                if( !assertion ) throw new DesignByContractException( message );
            }
            else
            {
                Trace.Assert( assertion, "Precondition: " + message );
            }
        }

        /// <summary>
        /// Precondition check - should run regardless of preprocessor directives.
        /// </summary>
        [DebuggerStepThrough]
        public static void Require( bool assertion, string message, Exception inner )
        {
            if( UseExceptions )
            {
                if( !assertion ) throw new DesignByContractException( message, inner );
            }
            else
            {
                Trace.Assert( assertion, "Precondition: " + message );
            }
        }

        /// <summary>
        /// Postcondition check.
        /// </summary>
        [DebuggerStepThrough]
        public static void Ensure( bool assertion, string message )
        {
            if( UseExceptions )
            {
                if( !assertion ) throw new DesignByContractException( message );
            }
            else
            {
                Trace.Assert( assertion, "Postcondition: " + message );
            }
        }

        /// <summary>
        /// Postcondition check.
        /// </summary>
        [DebuggerStepThrough]
        public static void Ensure( bool assertion, string message, Exception inner )
        {
            if( UseExceptions )
            {
                if( !assertion ) throw new DesignByContractException( message, inner );
            }
            else
            {
                Trace.Assert( assertion, "Postcondition: " + message );
            }
        }

        /// <summary>
        /// Postcondition check.
        /// </summary>
        [DebuggerStepThrough]
        public static void Ensure( bool assertion )
        {
            if( UseExceptions )
            {
                if( !assertion ) throw new DesignByContractException( "Postcondition failed." );
            }
            else
            {
                Trace.Assert( assertion, "Postcondition failed." );
            }
        }

        /// <summary>
        /// Invariant check.
        /// </summary>
        [DebuggerStepThrough]
        public static void Invariant( bool assertion )
        {
            if( UseExceptions )
            {
                if( !assertion ) throw new DesignByContractException( "Invariant failed." );
            }
            else
            {
                Trace.Assert( assertion, "Invariant failed." );
            }
        }

        /// <summary>
        /// Invariant check.
        /// </summary>
        [DebuggerStepThrough]
        public static void Invariant( bool assertion, string message )
        {
            if( UseExceptions )
            {
                if( !assertion ) throw new DesignByContractException( message );
            }
            else
            {
                Trace.Assert( assertion, "Invariant: " + message );
            }
        }

        /// <summary>
        /// Invariant check.
        /// </summary>
        [DebuggerStepThrough]
        public static void Invariant( bool assertion, string message, Exception inner )
        {
            if( UseExceptions )
            {
                if( !assertion ) throw new DesignByContractException( message, inner );
            }
            else
            {
                Trace.Assert( assertion, "Invariant: " + message );
            }
        }

        /// <summary>
        /// Assertion check.
        /// </summary>
        [DebuggerStepThrough]
        public static void Assert( bool assertion )
        {
            if( UseExceptions )
            {
                if( !assertion ) throw new DesignByContractException( "Assertion failed." );
            }
            else
            {
                Trace.Assert( assertion, "Assertion failed." );
            }
        }

        /// <summary>
        /// Assertion check.
        /// </summary>
        [DebuggerStepThrough]
        public static void Assert( bool assertion, string message )
        {
            if( UseExceptions )
            {
                if( !assertion ) throw new DesignByContractException( message );
            }
            else
            {
                Trace.Assert( assertion, "Assertion: " + message );
            }
        }

        /// <summary>
        /// Assertion check.
        /// </summary>
        [DebuggerStepThrough]
        public static void Assert( bool assertion, string message, Exception inner )
        {
            if( UseExceptions )
            {
                if( !assertion ) throw new DesignByContractException( message, inner );
            }
            else
            {
                Trace.Assert( assertion, "Assertion: " + message );
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Set this if you wish to use Trace Assert statements 
        /// instead of exception handling. 
        /// (The Check class uses exception handling by default.)
        /// </summary>
        public static bool UseAssertions { get; set; }

        /// <summary>
        /// Is exception handling being used?
        /// </summary>
        private static bool UseExceptions
        {
            [DebuggerStepThrough]
            get { return !UseAssertions; }
        }

        #endregion
    }
}
