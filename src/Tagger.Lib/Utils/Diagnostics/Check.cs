//-----------------------------------------------------------------------
// <copyright file="Check.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Utils.Diagnostics
{
    using System.Diagnostics;

    /// <summary>
    /// Design by contract checks.
    /// </summary>
    /// <remarks>
    /// Original from NHibernate Best Practices article on CodeProject
    /// </remarks>
    public static class Check
    {
        /// <summary>
        /// Precondition check - should run regardless of preprocessor directives.
        /// </summary>
        /// <param name="assertion">Statement to check</param>
        [DebuggerStepThrough]
        public static void Require(bool assertion)
        {
            if (!assertion)
            {
                throw new DesignByContractException("Precondition failed.");
            }
        }

        /// <summary>
        /// Precondition check - should run regardless of preprocessor directives.
        /// </summary>
        /// <param name="assertion">Statement to check</param>
        /// <param name="message">Additional information</param>
        [DebuggerStepThrough]
        public static void Require(bool assertion, string message)
        {
            if (!assertion)
            {
                throw new DesignByContractException(message);
            }
        }

        /// <summary>
        /// Postcondition check.
        /// </summary>
        /// <param name="assertion">Statement to check</param>
        [DebuggerStepThrough]
        public static void Ensure(bool assertion)
        {
            if (!assertion)
            {
                throw new DesignByContractException("Postcondition failed.");
            }
        }

        /// <summary>
        /// Postcondition check.
        /// </summary>
        /// <param name="assertion">Statement to check</param>
        /// <param name="message">Additional information</param>
        [DebuggerStepThrough]
        public static void Ensure(bool assertion, string message)
        {
            if (!assertion)
            {
                throw new DesignByContractException(message);
            }
        }

        /// <summary>
        /// Invariant check.
        /// </summary>
        /// <param name="assertion">Statement to check</param>
        [DebuggerStepThrough]
        public static void Invariant(bool assertion)
        {
            if (!assertion)
            {
                throw new DesignByContractException("Invariant failed.");
            }
        }

        /// <summary>
        /// Invariant check.
        /// </summary>
        /// <param name="assertion">Statement to check</param>
        /// <param name="message">Additional information</param>
        [DebuggerStepThrough]
        public static void Invariant(bool assertion, string message)
        {
            if (!assertion)
            {
                throw new DesignByContractException(message);
            }
        }

        /// <summary>
        /// Assertion check.
        /// </summary>
        /// <param name="assertion">Statement to check</param>
        [DebuggerStepThrough]
        public static void Assert(bool assertion)
        {
            if (!assertion)
            {
                throw new DesignByContractException("Assertion failed.");
            }
        }

        /// <summary>
        /// Assertion check.
        /// </summary>
        /// <param name="assertion">Statement to check</param>
        /// <param name="message">Additional information</param>
        [DebuggerStepThrough]
        public static void Assert(bool assertion, string message)
        {
            if (!assertion)
            {
                throw new DesignByContractException(message);
            }
        }
    }
}
