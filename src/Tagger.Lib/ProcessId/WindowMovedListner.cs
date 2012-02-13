using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManagedWinapi.Accessibility;

namespace Tagger
{
    public sealed class WindowMovedListner: IDisposable
    {
        private AccessibleEventListener m_Listner;

        public WindowMovedListner(IntPtr handle)
        {
            this.HostHandle = handle;

            uint pid = ProcessFinder.GetPid(HostHandle);

            // Hook pid (available only in Windowed process)
            // TODO: Catch destruction of another window as well
            // TODO: Dispose on exit
            m_Listner = new AccessibleEventListener
            {
                MinimalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                MaximalEventType = AccessibleEventType.EVENT_OBJECT_LOCATIONCHANGE,
                ProcessId = pid,
                Enabled = true,
            };
            m_Listner.EventOccurred += (object sender, AccessibleEventArgs e) =>
            {
                // Ignore events from cursor
                if (e.ObjectID != 0) { return; }

                if (this.Moved != null)
                {
                    this.Moved(this, EventArgs.Empty);
                }
            };
        }

        public IntPtr HostHandle { get; set; }

        public event EventHandler Moved;

        public void Dispose()
        {
            m_Listner.Dispose();
        }
    }
}
