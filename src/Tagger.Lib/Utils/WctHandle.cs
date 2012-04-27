//-----------------------------------------------------------------------
// <copyright file="WctHandle.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Tagger
{
    using Microsoft.Win32.SafeHandles;
    using Tagger.WinAPI;

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
        /// Prevents a default instance of the <see cref="WctHandle"/> class from being created. 
        /// </summary>
        private WctHandle()
            : base(true)
        {
        }

        /// <summary>
        /// Executes the code required to free the handle
        /// </summary>
        /// <returns>
        /// true if the handle is released successfully
        /// </returns>
        protected override bool ReleaseHandle()
        {
            NativeMethods.CloseThreadWaitChainSession(this.handle);
            return true;
        }
    }
}