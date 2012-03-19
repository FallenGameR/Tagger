using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Tagger.WinAPI;
using Utils.Extensions;
using System.Windows.Media;
using System.Windows.Documents;
using System.Collections.Generic;
using Utils.Diagnostics;

namespace Tagger.Dwm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IntPtr thumbnailHandle;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += delegate { this.RebindWindowItems(); };
        }

        private IntPtr Handle
        {
            get { return new WindowInteropHelper(this).Handle; }
        }
        
        /// <summary>
        /// NOTE: Would not be reused
        /// </summary>
        private void RebindWindowItems()
        {
            lstWindows.Items.Clear();

            var success = NativeAPI.EnumWindows( (hwnd, lParam) =>
            {
                var windowLong = NativeAPI.GetWindowLong(hwnd, NativeAPI.GWL_STYLE);
                if (windowLong == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                const ulong TARGETWINDOW = NativeAPI.WS_BORDER | NativeAPI.WS_VISIBLE;

                if (this.Handle != hwnd && (windowLong & TARGETWINDOW) == TARGETWINDOW)
                {
                    var sb = new StringBuilder(100);
                    var ress = NativeAPI.GetWindowText(hwnd, sb, sb.Capacity);
                    var err = Marshal.GetLastWin32Error();
                    if ((ress == 0) && ( err != NativeAPI.NO_ERROR))
                    {
                        throw new Win32Exception(err);
                    }

                    lstWindows.Items.Add(new WindowItem
                    {
                        Handle = hwnd,
                        Title = sb.ToString(),
                    });
                }

                return true; //continue enumeration
            }, 0);

            if (success == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

                
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.thumbnailHandle != IntPtr.Zero)
            {
                var hresult = NativeAPI.DwmUnregisterThumbnail(this.thumbnailHandle);
                if (hresult != NativeAPI.S_OK)
                {
                    throw new COMException("DwmUnregisterThumbnail( {0} ) failed".Format(this.thumbnailHandle), hresult);
                }
            }

            this.RebindWindowItems();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var window = (WindowItem)lstWindows.SelectedItem;

            if (this.thumbnailHandle != IntPtr.Zero)
            {
                var hresultUnregister = NativeAPI.DwmUnregisterThumbnail(thumbnailHandle);
                if (hresultUnregister != NativeAPI.S_OK)
                {
                    throw new COMException("DwmUnregisterThumbnail failed", hresultUnregister);
                }
            }

            var hresultRegister = NativeAPI.DwmRegisterThumbnail(this.Handle, window.Handle, out thumbnailHandle);

            if (hresultRegister == NativeAPI.S_OK)
            {
                this.UpdateThumb();
            }
            else
            {
                throw new COMException("DwmRegisterThumbnail( {0}, {1}, ... )".Format(this.Handle, window.Handle), hresultRegister);
            }
        }

        private void UpdateThumb()
        {
            if (this.thumbnailHandle == IntPtr.Zero) { return; }

            NativeAPI.SIZE thumbnailSize;
            var hresultQuery = NativeAPI.DwmQueryThumbnailSourceSize(thumbnailHandle, out thumbnailSize);
            if (hresultQuery != NativeAPI.S_OK)
            {
                throw new COMException("DwmQueryThumbnailSourceSize( {0}, ... ) failed".Format(thumbnailHandle), hresultQuery);
            }

            var location = canvas.GetLocation();
            var properties = new NativeAPI.DWM_THUMBNAIL_PROPERTIES
            {
                fVisible = true,
                dwFlags = NativeAPI.DWM_TNP_VISIBLE | NativeAPI.DWM_TNP_RECTDESTINATION,
                rcDestination = new NativeAPI.RECT
                {
                    Left = (int)location.X,
                    Top = (int)location.Y,
                    Right = (int)(location.X + Math.Min(canvas.ActualWidth, thumbnailSize.x)),
                    Bottom = (int)(location.Y + Math.Min(canvas.ActualHeight, thumbnailSize.y)),
                },
            };

            var hresultUpdate = NativeAPI.DwmUpdateThumbnailProperties(thumbnailHandle, ref properties);
            if (hresultUpdate != NativeAPI.S_OK)
            {
                throw new COMException("DwmUpdateThumbnailProperties( {0}, ... ) failed".Format(thumbnailHandle), hresultUpdate);
            }
        }

        private void ComboBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateThumb();
        }
    }
}
