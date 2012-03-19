using System;
using System.Runtime.InteropServices;
using System.Windows;
using Tagger.WinAPI;
using Utils.Diagnostics;
using Utils.Extensions;

namespace Tagger.Dwm
{
    /// <summary>
    /// Thumbnail provided by Desktop Window Manager
    /// </summary>
    public sealed class Thumbnail : IDisposable
    {
        /// <summary>
        /// Handle to DWM thumbnail
        /// </summary>
        private IntPtr thumbnailHandle;

        /// <summary>
        /// WPF control that is used to render the thumbnail
        /// </summary>
        private FrameworkElement destinationControl;

        /// <summary>
        /// Initializes a new instance of Thumbnail class
        /// </summary>
        /// <param name="source">Source window handle that is used to generate thumbnail</param>
        /// <param name="destination">Destination WPF control that is used to render thumbnail</param>
        public Thumbnail(IntPtr source, FrameworkElement destination)
        {
            Check.Require(source != IntPtr.Zero, "Source must not be zero");
            Check.Require(destination != null, "Destination must not be null");
            this.destinationControl = destination;

            var destinationWindow = (Window)destination.GetTopLevelElement();
            var destinationHandle = destinationWindow.GetHandle();
            var hresultRegister = NativeAPI.DwmRegisterThumbnail(destinationHandle, source, out this.thumbnailHandle);

            if (hresultRegister == NativeAPI.S_OK)
            {
                this.UpdateOnDestinationSizeChanged();
                this.destinationControl.SizeChanged += delegate { this.UpdateOnDestinationSizeChanged(); };
            }
            else
            {
                // TODO: Implement more sound way of handling source destroyed events
                //throw new COMException("DwmRegisterThumbnail( {0}, {1}, ... )".Format(destinationHandle, source), hresultRegister);
            }
        }

        /// <summary>
        /// Cleanup allocated resources
        /// </summary>
        public void Dispose()
        {
            var hresultUnregister = NativeAPI.DwmUnregisterThumbnail(this.thumbnailHandle);

            if (hresultUnregister != NativeAPI.S_OK)
            {
                // TODO: Implement more sound way of handling source destroyed events
                //throw new COMException("DwmUnregisterThumbnail( {0} ) failed".Format(this.thumbnailHandle), hresultUnregister);
            }
        }

        /// <summary>
        /// Update thumbnail on destination WPF control size changed
        /// </summary>
        private void UpdateOnDestinationSizeChanged()
        {
            NativeAPI.SIZE thumbnailSize;
            var hresultQuery = NativeAPI.DwmQueryThumbnailSourceSize(this.thumbnailHandle, out thumbnailSize);
            if (hresultQuery != NativeAPI.S_OK)
            {
                throw new COMException("DwmQueryThumbnailSourceSize( {0}, ... ) failed".Format(this.thumbnailHandle), hresultQuery);
            }

            var location = this.destinationControl.GetLocation();
            var properties = new NativeAPI.DWM_THUMBNAIL_PROPERTIES
            {
                fVisible = true,
                dwFlags = NativeAPI.DWM_TNP_VISIBLE | NativeAPI.DWM_TNP_RECTDESTINATION,
                rcDestination = new NativeAPI.RECT
                {
                    Left = (int)location.X,
                    Top = (int)location.Y,
                    Right = (int)(location.X + Math.Min(this.destinationControl.ActualWidth, thumbnailSize.x)),
                    Bottom = (int)(location.Y + Math.Min(this.destinationControl.ActualHeight, thumbnailSize.y)),
                },
            };

            var hresultUpdate = NativeAPI.DwmUpdateThumbnailProperties(this.thumbnailHandle, ref properties);
            if (hresultUpdate != NativeAPI.S_OK)
            {
                throw new COMException("DwmUpdateThumbnailProperties( {0}, ... ) failed".Format(this.thumbnailHandle), hresultUpdate);
            }
        }
    }
}
