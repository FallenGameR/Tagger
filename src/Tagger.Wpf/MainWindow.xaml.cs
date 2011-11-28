using System.Windows;
using ManagedWinapi.Accessibility;
using WinAPI.WaitChainTraversal;

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
        }

        AccessibleEventListener listner;
        private void Window_Loaded( object sender, RoutedEventArgs e )
        {
            var finder = new ProcessFinder();
            var pid = finder.FindHostProcess( 2728 );

            listner = new AccessibleEventListener
            {
                MinimalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                MaximalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                ProcessId = (uint) pid,
                Enabled = true,
            };

            listner.EventOccurred += new AccessibleEventHandler( listner_EventOccurred );
            listner_EventOccurred( null, null );
        }

        void listner_EventOccurred( object sender, AccessibleEventArgs e )
        {
            if( e == null )
            {
                MessageBox.Show( "1" );
                return;
            }

            // Ignore events from cursor
            if( e.ObjectID != 0 ) { return; }

            var rect = e.AccessibleObject.Location;
        //            public SystemAccessibleObject AccessibleObject { get; }
        //public uint ChildID { get; }
        //public AccessibleEventType EventType { get; }
        //public IntPtr HWnd { get; }
        //public uint ObjectID { get; }
        //public uint Thread { get; }
        //public uint Time { get; }

            // OBJID_WINDOW == 0
            test.Text = string.Format( @"
{0}
{1}
", 
                e.ObjectID.ToString()
                ,
                e.AccessibleObject.Location );
        }

    }
}
