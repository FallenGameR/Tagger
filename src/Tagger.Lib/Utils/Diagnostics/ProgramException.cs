using System;
using System.Runtime.Serialization;

namespace Utils.Diagnostics
{
    /// <summary>
    /// Root of all program's exceptions
    /// </summary>
    [Serializable]
    public class ProgramException : ApplicationException
    {
        public ProgramException() { }
        public ProgramException(string message) : base(message) { }
        public ProgramException(string message, Exception inner) : base(message, inner) { }
        protected ProgramException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
