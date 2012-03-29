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
        /// Application settings window 
        /// </summary>
        public static Window MainSettingsWindow { get; private set; }

        /// <summary>
        /// Initiailize application scale events
        /// </summary>
        protected override void OnStartup(StartupEventArgs ea)
        {
            base.OnStartup(ea);

            // Register unhandled exception handler with logging
            App.Current.DispatcherUnhandledException += (sender, args) =>
            {
                var filePath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "LastUnhandledException.txt");

                File.WriteAllText(filePath, args.Exception.ToString());

                MessageBox.Show(
                    args.Exception.Message,
                    "Unhandled exception",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                args.Handled = false;
            };

            // Create and show start window
            App.MainSettingsWindow = new GlobalSettingsWindow();
            App.MainSettingsWindow.Show();
        }
    }
}
