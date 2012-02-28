using Microsoft.Win32.SafeHandles;
using Tagger.WinAPI;

namespace Tagger
{
    /// <summary>
    /// Safe handle that is used in wait chain traversal API calls
    /// </summary>
    /// <remarks>
    /// See http://msdn.microsoft.com/en-us/library/system.runtime.interopservices.safehandle.aspx
    /// to learn why SafeHandle descendant should be used instead of raw IntPtr
    /// </remarks>
    public class WctHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        /// <summary>
        /// Initializes handle class
        /// </summary>
        private WctHandle() : base(true) { }

        /// <summary>
        /// Executes the code required to free the handle
        /// </summary>
        /// <returns>
        /// true if the handle is released successfully
        /// </returns>
        protected override bool ReleaseHandle()
        {
            NativeAPI.CloseThreadWaitChainSession(this.handle);
            return true;
        }
    }
}
