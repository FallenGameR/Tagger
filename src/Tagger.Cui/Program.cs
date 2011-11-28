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
            var pid = 2728;
            var tids = Process.GetProcessById( pid ).Threads.Cast<ProcessThread>().Select( t => t.Id );


            var wct = new WaitChainTraversal();
            foreach( var tid in tids )
            {
                try
                {
                    var conhost = wct.FindParentConhost( tid );
                    Console.WriteLine( conhost );
                }
                catch( Exception ex )
                {
                    Console.WriteLine( ex.Message );
                }
            }
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
