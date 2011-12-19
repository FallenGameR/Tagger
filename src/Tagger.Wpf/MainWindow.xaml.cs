using System.Windows;
using ManagedWinapi.Accessibility;
using System;
using Tagger.WinAPI.WaitChainTraversal;
using Tagger.Lib;

namespace Tagger.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: Window
    {
        public MainWindow()
        {
            // Check windows edition
            InitializeComponent();

            // Prepopulate compiled assebly cache
            //Utils.PreloadAccessibilityAssembly();

            // If console app
            // get conhost pid

            // Hook pid (available only in Windowed process)

            /*
            Console.WriteLine(Utils.IsConsoleApp(9768));
            Console.WriteLine(Utils.IsConsoleApp(6620));

            return;

            var finder = new ProcessFinder();
            var start = DateTime.Now;
            var window = finder.GetConhostProcess(6572);
            Console.WriteLine(window);
            Console.WriteLine(DateTime.Now - start);

            return;

            var listner = new AccessibleEventListener
            {
                MinimalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                MaximalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                ProcessId = 2728,
                Enabled = true,
            };

            listner.EventOccurred += new AccessibleEventHandler(listner_EventOccurred);
            /**/
        }

        AccessibleEventListener listner2;

        void listner_EventOccurred( object sender, AccessibleEventArgs e )
        {
            // Ignore events from cursor
            if( e.ObjectID != 0 ) { return; }
            txtInfo.Text = e.AccessibleObject.Location.ToString();
            Console.WriteLine("!");
        }

        private void btnApply1_Click( object sender, RoutedEventArgs e )
        {
        }

        private void btnApply2_Click( object sender, RoutedEventArgs e )
        {
            var finder = new ProcessFinder();
            var pid = 0; // finder.GetConhostProcess(int.Parse(txtPid2.Text));

            listner2 = new AccessibleEventListener
            {
                MinimalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                MaximalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                ProcessId = (uint) pid,
                Enabled = true,
            };
            listner2.EventOccurred += new AccessibleEventHandler( listner_EventOccurred );
        }
    }
}
