using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Tagger.WinAPI;

namespace Tagger.Dwm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Thumbnail thumbnail;

        public MainWindow()
        {
            InitializeComponent();

            // NOTE: Would not be reused
            this.Loaded += delegate
            {
                lstWindows.Items.Clear();

                var success = NativeAPI.EnumWindows((hwnd, lParam) =>
                {
                    var windowLong = NativeAPI.GetWindowLong(hwnd, NativeAPI.GWL_STYLE);
                    if (windowLong == 0)
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }

                    const ulong TARGETWINDOW = NativeAPI.WS_BORDER | NativeAPI.WS_VISIBLE;
                    var handle = new WindowInteropHelper(this).Handle;

                    if (handle != hwnd && (windowLong & TARGETWINDOW) == TARGETWINDOW)
                    {
                        var sb = new StringBuilder(100);
                        var ress = NativeAPI.GetWindowText(hwnd, sb, sb.Capacity);
                        var err = Marshal.GetLastWin32Error();
                        if ((ress == 0) && (err != NativeAPI.NO_ERROR))
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
            };
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.thumbnail != null)
            {
                this.thumbnail.Dispose();
            }

            var window = (WindowItem)lstWindows.SelectedItem;
            this.thumbnail = new Thumbnail(window.Handle, canvas);
        }
    }
}
