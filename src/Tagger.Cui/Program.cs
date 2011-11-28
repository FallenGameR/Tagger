using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManagedWinapi.Accessibility;
using WinAPI.WaitChainTraversal;
using System.Diagnostics;

namespace Tagger.Cui
{
    class Program
    {
        static void Main( string[] args )
        {


            var wct = new WaitChainTraversal();
            var window = wct.FindWindowPid( 6572 );
            Console.WriteLine(window);

            return;

            var listner = new AccessibleEventListener
            {
                MinimalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                MaximalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                ProcessId = 2728,
                Enabled = true,
            };

            listner.EventOccurred += new AccessibleEventHandler( listner_EventOccurred );
        }

        static void listner_EventOccurred( object sender, AccessibleEventArgs e )
        {
            Console.WriteLine( "!" );
        }
    }
}
