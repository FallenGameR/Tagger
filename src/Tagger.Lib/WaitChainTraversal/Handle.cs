namespace WinAPI.WaitChainTraversal
{
    internal class Handle: Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid
    {
        private Handle() : base( true ) { }

        /// <summary>
        /// Executes the code required to free the handle
        /// </summary>
        /// <returns>
        /// true if the handle is released successfully
        /// </returns>
        protected override bool ReleaseHandle()
        {
            NativeAPI.CloseThreadWaitChainSession( this.handle );
            return true;
        }
    }
}
