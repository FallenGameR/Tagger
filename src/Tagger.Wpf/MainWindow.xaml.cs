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
            InitializeComponent();
            Utils.PreloadAccessibilityAssembly();
        }

        AccessibleEventListener listner2;

        void listner_EventOccurred( object sender, AccessibleEventArgs e )
        {
            // Ignore events from cursor
            if( e.ObjectID != 0 ) { return; }
            txtInfo.Text = e.AccessibleObject.Location.ToString();
        }

        private void btnApply1_Click( object sender, RoutedEventArgs e )
        {
        }

        private void btnApply2_Click( object sender, RoutedEventArgs e )
        {
            var finder = new ProcessFinder();
            var pid = finder.FindHostProcess( int.Parse( txtPid2.Text ) );

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
