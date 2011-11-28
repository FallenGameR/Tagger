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
            var finder = new ProcessFinder();
            var start = DateTime.Now;
            var window = finder.FindHostProcess( 6572 );
            Console.WriteLine(window);
            Console.WriteLine( DateTime.Now - start );

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
