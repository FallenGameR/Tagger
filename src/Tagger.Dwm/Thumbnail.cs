using System;
using System.Runtime.InteropServices;
using System.Windows;
using Tagger.WinAPI;
using Utils.Diagnostics;
using Utils.Extensions;

namespace Tagger.Dwm
{
    public sealed class Thumbnail : IDisposable
    {
        private IntPtr thumbnailHandle;
        private FrameworkElement destinationControl;

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
                this.UpdateThumb();
            }
            else
            {
                throw new COMException("DwmRegisterThumbnail( {0}, {1}, ... )".Format(destinationHandle, source), hresultRegister);
            }
        }

        public void Dispose()
        {
            var hresultUnregister = NativeAPI.DwmUnregisterThumbnail(this.thumbnailHandle);

            if (hresultUnregister != NativeAPI.S_OK)
            {
                throw new COMException("DwmUnregisterThumbnail( {0} ) failed".Format(this.thumbnailHandle), hresultUnregister);
            }
        }

        private void UpdateThumb()
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
