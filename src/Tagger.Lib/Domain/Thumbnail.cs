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
            this.SuccessfullyRegistered = hresultRegister == NativeAPI.S_OK;

            if (this.SuccessfullyRegistered)
            {
                this.SizeChangedHandler(null, null);
                this.destinationControl.SizeChanged += this.SizeChangedHandler;
            }
        }

        /// <summary>
        /// Gets a value indicating whether thumbnail was successfully registered
        /// </summary>
        /// <remarks>
        /// It is pointless to implement logic to handle source window destruction 
        /// more gracefully. Not showing thumbnail is gracefull enough strategy already.
        /// </remarks>
        public bool SuccessfullyRegistered { get; private set; }

        /// <summary>
        /// Cleanup allocated resources
        /// </summary>
        public void Dispose()
        {
            if (this.SuccessfullyRegistered)
            {
                NativeAPI.DwmUnregisterThumbnail(this.thumbnailHandle);
                this.destinationControl.SizeChanged -= this.SizeChangedHandler;
            }
        }

        /// <summary>
        /// Update thumbnail on destination WPF control size changed event
        /// </summary>
        private void SizeChangedHandler(object sender, SizeChangedEventArgs ea)
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
