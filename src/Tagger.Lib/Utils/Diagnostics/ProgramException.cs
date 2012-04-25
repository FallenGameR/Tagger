//-----------------------------------------------------------------------
// <copyright file="ProgramException.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Utils.Diagnostics
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Root of all program's exceptions
    /// </summary>
    [Serializable]
    public class ProgramException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramException"/> class.
        /// </summary>
        public ProgramException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramException"/> class.
        /// </summary>
        /// <param name="message">Exception message</param>
        public ProgramException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramException"/> class.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="inner">Inner exception</param>
        public ProgramException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramException"/> class.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected ProgramException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
