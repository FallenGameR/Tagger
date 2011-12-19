using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.Prism;
using Utils.Reflection;

namespace Tagger.Wpf
{
    public class HookViewModel : ViewModelBase
    {
        private int m_ProcessId;
        private string m_ApplicationType;
        private bool m_IsHooked;
        private string m_LastKnownPosition;

        /// <summary>
        /// Validation for ProcessId property value
        /// </summary>
        private void Validate_ProcessId()
        {
            Validate(m_ProcessId > 0, "Process ID must be greater than 0");
        }

        /// <summary>
        /// ID of the process that is being hooked
        /// </summary>
        public int ProcessId
        {
            get { return m_ProcessId; }
            set { m_ProcessId = value; OnPropertyChanged(this.Property(() => ProcessId)); }
        }

        /// <summary>
        /// Type of the application - is it Console or GUI application
        /// </summary>
        public string ApplicationType
        {
            get { return m_ApplicationType; }
            private set { m_ApplicationType = value; OnPropertyChanged(this.Property(() => ApplicationType)); }
        }

        /// <summary>
        /// True if the hook was applied on the application
        /// </summary>
        public bool IsHooked
        {
            get { return m_IsHooked; }
            private set { m_IsHooked = value; OnPropertyChanged(this.Property(() => IsHooked)); }
        }

        /// <summary>
        /// Last known position of application window
        /// </summary>
        public string LastKnownPosition
        {
            get { return m_LastKnownPosition; }
            private set { m_LastKnownPosition = value; OnPropertyChanged(this.Property(() => LastKnownPosition)); }
        }
    }
}
