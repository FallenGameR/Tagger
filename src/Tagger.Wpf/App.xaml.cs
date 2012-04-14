using System.IO;
using System.Reflection;
using System.Windows;
using System;

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

            // Check windows OS edition
            var isWindows7orWin2008R2orHigher = 
                Environment.OSVersion.Version.Major >= 6 && 
                Environment.OSVersion.Version.Minor >= 1;
            if ( !isWindows7orWin2008R2orHigher )
            {
                MessageBox.Show(@"
This program was tested only on Windows 7 and Windows 
2008 Server R2. To track console windows it uses conhost.exe
process that was not used in previous editions of Windows. 
To find mapping between conhost.exe and console application 
it uses Wait Chain Traversal API that was introduced only in
Windows Vista.

Meaning:
- If you are running Windows Vista, window tracking most likelly
wouldn't work for console windows.
- If you are using some presious version of Windows, you'll get
exception on the first attempt to create a tag.

If you want to add support for some old OS, fork Tagger  
https://github.com/FallenGameR/Tagger
",
                    "Windows Version Compatibility",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            // Create and show start window
            App.MainSettingsWindow = new TaggerSettingsWindow();
            App.MainSettingsWindow.Show();
        }
    }
}
