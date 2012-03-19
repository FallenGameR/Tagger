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

        private IntPtr Handle
        {
            get
            {
                return new WindowInteropHelper(this).Handle; ;
            }
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
                WinApi.PSIZE size;
                WinApi.DwmQueryThumbnailSourceSize(thumb, out size);

                WinApi.DWM_THUMBNAIL_PROPERTIES props = new WinApi.DWM_THUMBNAIL_PROPERTIES();

                props.fVisible = true;
                props.dwFlags = WinApi.DWM_TNP_VISIBLE | WinApi.DWM_TNP_RECTDESTINATION | WinApi.DWM_TNP_OPACITY;
                props.opacity = (byte)255;
                var position = GetPosition(image);
                props.rcDestination = new WinApi.Rect(
                    (int)position.X,
                    (int)position.Y,
                    (int)(position.X + 100),
                    (int)(position.Y + 100));

                if (size.x < image.Width)
                    props.rcDestination.Right = props.rcDestination.Left + size.x;

                if (size.y < image.Height)
                    props.rcDestination.Bottom = props.rcDestination.Top + size.y;

                WinApi.DwmUpdateThumbnailProperties(thumb, ref props);
            }
        }

        private Point GetPosition(Visual element)
        {
            var positionTransform = element.TransformToAncestor(this);
            var areaPosition = positionTransform.Transform(new Point(0, 0));

            return areaPosition;
        }

        private void ComboBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateThumb();
        }
    }
}
