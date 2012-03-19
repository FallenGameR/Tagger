using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Tagger.WinAPI;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Tagger.Dwm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IntPtr thumb;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += delegate { this.GetWindows(); };
        }

        private IntPtr Handle
        {
            get { return new WindowInteropHelper(this).Handle; }
        }
        
        private void GetWindows()
        {
            var windows = new List<WindowItem>();
            var res = NativeAPI.EnumWindows( (IntPtr hwnd, int lParam) =>
            {
                var winLong = NativeAPI.GetWindowLong(hwnd, NativeAPI.GWL_STYLE);
                if (winLong == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                if (this.Handle != hwnd && (winLong & NativeAPI.TARGETWINDOW) == NativeAPI.TARGETWINDOW)
                {
                    var sb = new StringBuilder(100);
                    var ress = NativeAPI.GetWindowText(hwnd, sb, sb.Capacity);
                    var err = Marshal.GetLastWin32Error();
                    if ((ress == 0) && ( err != NativeAPI.NO_ERROR))
                    {
                        throw new Win32Exception(err);
                    }

                    var t = new WindowItem();
                    t.Handle = hwnd;
                    t.Title = sb.ToString();
                    windows.Add(t);
                }

                return true; //continue enumeration
            }, 0);
            if (res == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            lstWindows.Items.Clear();
            foreach (WindowItem w in windows)
                lstWindows.Items.Add(w);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (thumb != IntPtr.Zero)
            {
                var res = NativeAPI.DwmUnregisterThumbnail(thumb);
                if (res != NativeAPI.S_OK)
                {
                    throw new COMException("DwmUnregisterThumbnail( )", res);
                }
            }

            GetWindows();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var w = (WindowItem)lstWindows.SelectedItem;

            if (thumb != IntPtr.Zero)
            {
                var res  = NativeAPI.DwmUnregisterThumbnail(thumb);
                if (res != NativeAPI.S_OK)
                {
                    throw new COMException("DwmUnregisterThumbnail failed", res);
                }
            }

            int i = NativeAPI.DwmRegisterThumbnail(this.Handle, w.Handle, out thumb);

            if (i == NativeAPI.S_OK)
            {
                UpdateThumb();
            }
            else
            {
                throw new COMException("DwmRegisterThumbnail failed", i);
            }
        }

        private void UpdateThumb()
        {
            if (thumb != IntPtr.Zero)
            {
                NativeAPI.SIZE thumbnailSize;
                var res = NativeAPI.DwmQueryThumbnailSourceSize(thumb, out thumbnailSize);
                if (res != NativeAPI.S_OK)
                {
                    throw new COMException("DwmQueryThumbnailSourceSize failed", res);
                }

                Point locationFromWindow = canvas.TranslatePoint(new Point(0, 0), this);

                var props = new NativeAPI.DWM_THUMBNAIL_PROPERTIES
                {
                    fVisible = true,
                    dwFlags = NativeAPI.DWM_TNP_VISIBLE | NativeAPI.DWM_TNP_RECTDESTINATION | NativeAPI.DWM_TNP_OPACITY,
                    opacity = byte.MaxValue,
                    rcDestination = new NativeAPI.RECT
                    {
                        Left = (int)locationFromWindow.X,
                        Top = (int)locationFromWindow.Y,
                        Right = (int)(locationFromWindow.X + Math.Min(canvas.ActualWidth, thumbnailSize.x)),
                        Bottom = (int)(locationFromWindow.Y + Math.Min(canvas.ActualHeight, thumbnailSize.y)),
                    },
                };

                var res2 = NativeAPI.DwmUpdateThumbnailProperties(thumb, ref props);
                if (res2 != NativeAPI.S_OK)
                {
                    throw new COMException("DwmUpdateThumbnailProperties", res2);
                }
            }
        }

        private void ComboBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateThumb();
        }
    }
}
