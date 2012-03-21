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
        /// <summary>
        /// A value for the fVisible member has been specified.
        /// </summary>
        public const int DWM_TNP_VISIBLE = 0x8;

        /// <summary>
        /// A value for the rcDestination member has been specified.
        /// </summary>
        public const int DWM_TNP_RECTDESTINATION = 0x1;

        /// <summary>
        /// The SIZE structure specifies the width and height of a rectangle.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE
        {
            public int x;
            public int y;
        }

        /// <summary>
        /// Specifies Desktop Window Manager (DWM) thumbnail properties. 
        /// </summary>
        /// <remarks>
        /// Used by the DwmUpdateThumbnailProperties function.
        /// </remarks>
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

        /// <summary>
        /// Specifies margin of the glass frame
        /// </summary>
        /// <remarks>
        /// Used in DwmExtendFrameIntoClientArea function.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int Left;
            public int Right;
            public int Top;
            public int Bottom;
        }

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
        /// Obtains a value that indicates whether Desktop Window Manager (DWM) composition is enabled. 
        /// </summary>
        /// <returns>
        /// true if composition is enabled.
        /// </returns>
        [DllImport("dwmapi.dll", PreserveSig=false)]
        public static extern bool DwmIsCompositionEnabled();

        /// <summary>
        /// Extends the window frame into the client area.
        /// </summary>
        /// <param name="hwnd">The handle to the window in which the frame will be extended into the client area.</param>
        /// <param name="margins">A pointer to a MARGINS structure that describes the margins to use when extending the frame into the client area.</param>
        [DllImport("dwmapi.dll", PreserveSig=false)]
        public static extern void DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);
    }
}
