//-----------------------------------------------------------------------
// <copyright file="TrayIconViewModel.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Tagger.ViewModels
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    using Microsoft.Practices.Prism.Commands;

    using Tagger.Wpf;

    using Utils.Prism;

    /// <summary>
    /// View model for TrayIcon control
    /// </summary>
    public class TrayIconViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of TrayIconViewModel class
        /// </summary>
        public TrayIconViewModel()
        {
            this.ShowSettingsCommand = new DelegateCommand<object>(delegate
            {
                App.MainSettingsWindow.Show();
                App.MainSettingsWindow.WindowState = System.Windows.WindowState.Normal;
                App.MainSettingsWindow.ShowInTaskbar = true;

            });
            this.BrowseSourcesCommand = new DelegateCommand<object>(delegate
            {
                Process.Start(@"https://github.com/FallenGameR/Tagger/");
            });
            this.CloseProgramCommand = new DelegateCommand<object>(delegate
            {
                // http://stackoverflow.com/questions/1867380/application-current-shutdown-doesnt
                new Thread(() =>
                    App.MainSettingsWindow.Dispatcher.BeginInvoke((Action)(() =>
                        App.Current.Shutdown()))
                ).Start();
            });
        }

        /// <summary>
        /// Show settings command 
        /// </summary>
        public DelegateCommand<object> ShowSettingsCommand { get; private set; }

        /// <summary>
        /// Browse project sources on github
        /// </summary>
        public DelegateCommand<object> BrowseSourcesCommand { get; private set; }

        /// <summary>
        /// Close program command
        /// </summary>
        public DelegateCommand<object> CloseProgramCommand { get; private set; }
    }
}

