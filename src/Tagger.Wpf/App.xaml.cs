using System.IO;
using System.Reflection;
using System.Windows;

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
                var filePath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "LastUnhandledException.txt");

                File.WriteAllText(filePath, ea.Exception.ToString());

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
