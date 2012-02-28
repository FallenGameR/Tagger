using System.Windows;
using System.Windows.Interop;
using Tagger.WinApi;
using System;

namespace Tagger.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: Window
    {
        public MainWindow()
        {
            // TODO: Check windows OS edition
            InitializeComponent();

            this.WindowStyle = System.Windows.WindowStyle.None;
            this.ShowInTaskbar = false;

            this.Closed += delegate { App.Current.Shutdown(); };
        }      

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = (int)NativeAPI.GetWindowLong(wndHelper.Handle, (int)NativeAPI.GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int)NativeAPI.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            NativeAPI.SetWindowLong(wndHelper.Handle, (int)NativeAPI.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
            }
        }
    }
}
