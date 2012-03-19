namespace Tagger.WinAPI
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// Desktop Window Manager related functions
    /// </summary>
    public static partial class NativeAPI
    {
        public const int S_OK = 0;
        public const int NO_ERROR = 0;

        [StructLayout(LayoutKind.Sequential)]
        public struct DWM_THUMBNAIL_PROPERTIES
        {
            public int dwFlags;
            public RECT rcDestination;
            public RECT rcSource;
            public byte opacity;
            public bool fVisible;
            public bool fSourceClientAreaOnly;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE
        {
            public int x;
            public int y;
        }

        public const int GWL_STYLE = -16;

        public const int DWM_TNP_VISIBLE = 0x8;
        public const int DWM_TNP_OPACITY = 0x4;
        public const int DWM_TNP_RECTDESTINATION = 0x1;

        public const ulong WS_VISIBLE = 0x10000000L;
        public const ulong WS_BORDER = 0x00800000L;

        public const ulong TARGETWINDOW = WS_BORDER | WS_VISIBLE;

        /// <summary>
        /// Creates a Desktop Window Manager (DWM) thumbnail relationship between the destination and source windows.
        /// </summary>
        /// <param name="hwndDestination">The handle to the window that will use the DWM thumbnail.</param>
        /// <param name="hwndSource">he handle to the window to use as the thumbnail source.</param>
        /// <param name="phThumbnailId">A pointer to a handle that, when this function returns successfully, represents the registration of the DWM thumbnail.</param>
        /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
        [DllImport("dwmapi.dll")]
        public static extern int DwmRegisterThumbnail(IntPtr hwndDestination, IntPtr hwndSource, out IntPtr phThumbnailId);

        /// <summary>
        /// Removes a Desktop Window Manager (DWM) thumbnail relationship created by the DwmRegisterThumbnail function.
        /// </summary>
        /// <param name="hThumbnailId">The handle to the thumbnail relationship to be removed.</param>
        /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
        [DllImport("dwmapi.dll")]
        public static extern int DwmUnregisterThumbnail(IntPtr hThumbnailId);

        /// <summary>
        /// Retrieves the source size of the Desktop Window Manager (DWM) thumbnail.
        /// </summary>
        /// <param name="hThumbnail">A handle to the thumbnail to retrieve the source window size from.</param>
        /// <param name="pSize">A pointer to a SIZE structure that, when this function returns successfully, receives the size of the source thumbnail.</param>
        /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
        [DllImport("dwmapi.dll")]
        public static extern int DwmQueryThumbnailSourceSize(IntPtr hThumbnail, out SIZE pSize);

        /// <summary>
        /// Updates the properties for a Desktop Window Manager (DWM) thumbnail.
        /// </summary>
        /// <param name="hThumbnailId">The handle to the DWM thumbnail to be updated.</param>
        /// <param name="ptnProperties">A pointer to a DWM_THUMBNAIL_PROPERTIES structure that contains the new thumbnail properties.</param>
        /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
        [DllImport("dwmapi.dll")]
        public static extern int DwmUpdateThumbnailProperties(IntPtr hThumbnailId, ref DWM_THUMBNAIL_PROPERTIES ptnProperties);

        /// <summary>
        /// Retrieves information about the specified window.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.</param>
        /// <param name="nIndex">The zero-based offset to the value to be retrieved.</param>
        /// <returns>
        /// If the function succeeds, the return value is the requested value.
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern ulong GetWindowLong(IntPtr hWnd, int nIndex);

        /// <summary>
        /// Enumerates all top-level windows on the screen by passing the handle to each window, in turn, to an application-defined callback function.
        /// </summary>
        /// <param name="lpEnumFunc">A pointer to an application-defined callback function. For more information, see EnumWindowsProc. </param>
        /// <param name="lParam">An application-defined value to be passed to the callback function. </param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int EnumWindows(EnumWindowsCallback lpEnumFunc, int lParam);

        /// <summary>
        /// An application-defined callback function used with the EnumWindows or EnumDesktopWindows function.
        /// </summary>
        /// <param name="hwnd">A handle to a top-level window.</param>
        /// <param name="lParam">The application-defined value given in EnumWindows or EnumDesktopWindows. </param>
        /// <returns>To continue enumeration, the callback function must return TRUE; to stop enumeration, it must return FALSE. </returns>
        public delegate bool EnumWindowsCallback(IntPtr hwnd, int lParam);

        /// <summary>
        /// Copies the text of the specified window's title bar (if it has one) into a buffer.
        /// </summary>
        /// <param name="hWnd">A handle to the window or control containing the text. </param>
        /// <param name="lpString">The buffer that will receive the text. If the string is as long or longer than the buffer, the string is truncated and terminated with a null character.</param>
        /// <param name="nMaxCount">The maximum number of characters to copy to the buffer, including the null character. If the text exceeds this limit, it is truncated.</param>
        /// <returns>
        /// If the function succeeds, the return value is the length, in characters, of the copied string, not including 
        /// the terminating null character. If the window has no title bar or text, if the title bar is empty, or if the 
        /// window or control handle is invalid, the return value is zero. To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
    }
}
