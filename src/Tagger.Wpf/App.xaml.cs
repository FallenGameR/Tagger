using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.IO.Compression;

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

            // Override assembly resolution to use deflated referenced dlls
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

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

        private static byte[] GetDeflatedAssembly(string assemblyName)
        {
            switch (assemblyName)
            {
                case "Hardcodet.Wpf.TaskbarNotification, Version=1.0.4.0, Culture=neutral, PublicKeyToken=null":
                    return Tagger.Properties.Resources.Hardcodet_Wpf_TaskbarNotification_dll;

                case "ManagedWinapi, Version=0.3.0.0, Culture=neutral, PublicKeyToken=null":
                    return Tagger.Properties.Resources.ManagedWinapi_dll;

                case "Microsoft.Practices.Prism, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35":
                    return Tagger.Properties.Resources.Microsoft_Practices_Prism_dll;

                case "Tagger.Lib, Version=0.9.5.0, Culture=neutral, PublicKeyToken=null":
                    return Tagger.Properties.Resources.Tagger_Lib_dll;

                case "WPFToolkit.Extended, Version=1.5.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f4":
                    return Tagger.Properties.Resources.WPFToolkit_Extended_dll;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Custom assembly resolver to use deflated referenced assemblies from resources
        /// </summary>
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var knownAssembly = GetDeflatedAssembly(args.Name);
            if (knownAssembly == null) { return null; }

            // Loading of deflated dll from resources
            using (var resource = new MemoryStream(knownAssembly))
            using (var deflated = new DeflateStream(resource, CompressionMode.Decompress))
            using (var reader = new BinaryReader(deflated))
            {
                // NOTE: 1Mb buffer should be OK for all referenced DLLs
                var one_megabyte = 1024 * 1024;
                var buffer = reader.ReadBytes(one_megabyte);
                return Assembly.Load(buffer);
            }
        }
    }
}
