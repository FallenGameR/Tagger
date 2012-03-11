using System.Windows;
using System.Windows.Threading;
using System.IO;

namespace Tagger.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App: Application
    {
        /// <summary>
        /// Initiailize application scale events
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            App.Current.DispatcherUnhandledException += (sender, ea) =>
            {
                File.WriteAllText("LastUnhandledException.txt", ea.Exception.ToString());
                MessageBox.Show(
                    ea.Exception.Message,
                    "Unhandled exception",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            };

            base.OnStartup(e);
        }
    }
}
