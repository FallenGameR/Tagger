using Microsoft.Win32.SafeHandles;
using Tagger.WinAPI;

namespace Tagger
{
    /// <summary>
    /// Safe handle that is used in wait chain traversal API calls
    /// </summary>
    /// <remarks>
    /// Closes WCT session upon exit
    /// TODO: Figure out if it's necessary to use such class in the first place
    /// </remarks>
    public class WctHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
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
