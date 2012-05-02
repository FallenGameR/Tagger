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
    using System.Windows;

    using Microsoft.Practices.Prism.Commands;
    using Tagger.Wpf;
    using Utils.Prism;

    /// <summary>
    /// View model for TrayIcon control
    /// </summary>
    public class TrayIconViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrayIconViewModel"/> class. 
        /// </summary>
        public TrayIconViewModel()
        {
            this.ShowSettingsCommand = new DelegateCommand<object>(delegate
            {
                App.MainSettingsWindow.Show();
                App.MainSettingsWindow.WindowState = WindowState.Normal;
                App.MainSettingsWindow.ShowInTaskbar = true;
            });
            this.BrowseSourcesCommand = new DelegateCommand<object>(delegate
            {
                Process.Start(@"https://github.com/FallenGameR/Tagger/");
            });
            this.CloseProgramCommand = new DelegateCommand<object>(delegate
            {
                // Application.Current.Shutdown doesn't work here - application wouldn't exit.
                // Solution with calling Shutdown from MainSettingsWindow Dispatcher also doesn't work.
                // More details - http://stackoverflow.com/questions/1867380/application-current-shutdown-doesnt
                // If you want to track this glitch try closing from tray with at least one tag registered.
                Process.GetCurrentProcess().Kill();
            });
        }

        /// <summary>
        /// Gets show settings command 
        /// </summary>
        public DelegateCommand<object> ShowSettingsCommand { get; private set; }

        /// <summary>
        /// Gets browse project sources on github command
        /// </summary>
        public DelegateCommand<object> BrowseSourcesCommand { get; private set; }

        /// <summary>
        /// Gets close program command
        /// </summary>
        public DelegateCommand<object> CloseProgramCommand { get; private set; }
    }
}