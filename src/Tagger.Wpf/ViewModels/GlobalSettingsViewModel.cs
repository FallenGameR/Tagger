using Utils.Prism;
using Utils.Extensions;

namespace Tagger.ViewModels
{
    /// <summary>
    /// View model for global settings window
    /// </summary>
    public class GlobalSettingsViewModel : ViewModelBase
    {
        private bool m_UseColorRandomization;

        /// <summary>
        /// Flag that tells if color randomization is used
        /// </summary>
        /// <remarks>
        /// If set all new tag would be initialized with random tag color
        /// </remarks>
        public bool UseColorRandomization
        {
            get { return m_UseColorRandomization; }
            set { m_UseColorRandomization = value; OnPropertyChanged(this.Property(() => UseColorRandomization)); }
        }
    }
}
