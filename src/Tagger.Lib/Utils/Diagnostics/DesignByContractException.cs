//-----------------------------------------------------------------------
// <copyright file="DesignByContractException.cs" company="none">
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
    /// Exception thrown when code contract is violated
    /// </summary>
    [Serializable]
    public class DesignByContractException : ProgramException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DesignByContractException"/> class.
        /// </summary>
        internal DesignByContractException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DesignByContractException"/> class.
        /// </summary>
        /// <param name="message">Exception message</param>
        internal DesignByContractException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DesignByContractException"/> class.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="inner">Inner exception</param>
        internal DesignByContractException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DesignByContractException"/> class.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        internal DesignByContractException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
