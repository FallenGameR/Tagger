using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;

namespace Tagger.Dwm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IntPtr thumb;
        private List<WindowItem> windows;

        public MainWindow()
        {
            InitializeComponent();
        }

        private IntPtr Handle
        {
            get { return new WindowInteropHelper(this).Handle; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.GetWindows();
        }

        private void GetWindows()
        {
            windows = new List<WindowItem>();
            WinApi.EnumWindows(Callback, 0);

            lstWindows.Items.Clear();
            foreach (WindowItem w in windows)
                lstWindows.Items.Add(w);
        }

        private bool Callback(IntPtr hwnd, int lParam)
        {
            if (this.Handle != hwnd && (WinApi.GetWindowLongA(hwnd, WinApi.GWL_STYLE) & WinApi.TARGETWINDOW) == WinApi.TARGETWINDOW)
            {
                var sb = new StringBuilder(100);
                WinApi.GetWindowText(hwnd, sb, sb.Capacity);

                var t = new WindowItem();
                t.Handle = hwnd;
                t.Title = sb.ToString();
                windows.Add(t);
            }

            return true; //continue enumeration
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (thumb != IntPtr.Zero)
                WinApi.DwmUnregisterThumbnail(thumb);

            GetWindows();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var w = (WindowItem)lstWindows.SelectedItem;

            if (thumb != IntPtr.Zero)
                WinApi.DwmUnregisterThumbnail(thumb);

            int i = WinApi.DwmRegisterThumbnail(this.Handle, w.Handle, out thumb);

            if (i == 0)
                UpdateThumb();
        }

        private void UpdateThumb()
        {
            if (thumb != IntPtr.Zero)
            {
                WinApi.PSIZE thumbnailSize;
                WinApi.DwmQueryThumbnailSourceSize(thumb, out thumbnailSize);

                Point locationFromWindow = canvas.TranslatePoint(new Point(0, 0), this);

                var props = new WinApi.DWM_THUMBNAIL_PROPERTIES
                {
                    fVisible = true,
                    dwFlags = WinApi.DWM_TNP_VISIBLE | WinApi.DWM_TNP_RECTDESTINATION | WinApi.DWM_TNP_OPACITY,
                    opacity = byte.MaxValue,
                    rcDestination = new WinApi.Rect
                    {
                        Left = (int)locationFromWindow.X,
                        Top = (int)locationFromWindow.Y,
                        Right = (int)(locationFromWindow.X + canvas.ActualWidth),
                        Bottom = (int)(locationFromWindow.Y + canvas.ActualHeight),
                    },
                };

                WinApi.DwmUpdateThumbnailProperties(thumb, ref props);
            }
        }

        private void ComboBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateThumb();
        }
    }
}
