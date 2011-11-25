using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ManagedWinapi.Accessibility;

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
            listner = new AccessibleEventListener
            {
                MinimalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                MaximalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                ProcessId = 5228,
                Enabled = true,
            };

            listner.EventOccurred += new AccessibleEventHandler( listner_EventOccurred );
        }

        void listner_EventOccurred( object sender, AccessibleEventArgs e )
        {
            // OBJID_WINDOW == 0
            test.Text = e.ObjectID.ToString();
        }

    }
}
