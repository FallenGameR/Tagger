//-----------------------------------------------------------------------
// <copyright file="GlobalSettingsViewModel.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Tagger.ViewModels
{
    using Tagger.Properties;
    using Utils.Extensions;
    using Utils.Prism;

    /// <summary>
    /// View model for global settings window
    /// </summary>
    public class GlobalSettingsViewModel : ViewModelBase
    {
        /// <summary>
        /// Flag that tells if color randomization is used
        /// </summary>
        /// <remarks>
        /// If set all new tag would be initialized with random tag color
        /// </remarks>
        public bool UseColorRandomization
        {
            get
            {
                return Settings.Default.GlobalSettings_UseColorRandomization;
            }
            set
            {
                Settings.Default.GlobalSettings_UseColorRandomization = value; 
                OnPropertyChanged(this.Property(() => UseColorRandomization));
            }
        }
    }
}

