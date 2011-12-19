using System;
using System.Runtime.Serialization;

namespace Utils.Diagnostics
{
    /// <summary>
    /// Exceptions when code contract is violated
    /// </summary>
    /// <remarks>
    /// Taken from NHibernate Best Practices (on CodeProject site)
    /// </remarks>
    [Serializable]
    public class DesignByContractException : ProgramException
    {
        internal DesignByContractException() { }
        internal DesignByContractException(string message) : base(message) { }
        internal DesignByContractException(string message, Exception inner) : base(message, inner) { }
        internal DesignByContractException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
