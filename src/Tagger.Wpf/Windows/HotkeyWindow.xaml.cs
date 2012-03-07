using System.Windows;
using System.Windows.Interop;
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

            this.Loaded += delegate { RegistrationManager.RegisterException(this); };
            this.Closed += delegate { App.Current.Shutdown(); };
        }      

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
                this.ShowInTaskbar = false;
            }
        }
    }
}
