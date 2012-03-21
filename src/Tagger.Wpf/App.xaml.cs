using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

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

            // Show start window
            var startWindow = new MainWindow();
            startWindow.Show();

            // If in debug mode, prepare test window set
            if (ea.Args.Any( a => a.ToLower() == "-debug" ) )
            {
                startWindow.WindowState = WindowState.Minimized;
                this.CreateTag("one", Colors.SeaGreen);
                this.CreateTag("one", Colors.SeaGreen);
                this.CreateTag("two", Colors.YellowGreen);
            }
        }

        private void CreateTag(string text, Color color)
        {
            Process.Start("notepad.exe").WaitForInputIdle();
            var context = RegistrationManager.TagRegistrationHandler(delegate { });
            context.SettingsWindow.Hide();
            context.TagViewModel.Text = text;
            context.TagViewModel.Color = color;
        }
    }
}
