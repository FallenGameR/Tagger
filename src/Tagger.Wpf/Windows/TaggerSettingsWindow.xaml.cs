//-----------------------------------------------------------------------
// <copyright file="TaggerSettingsWindow.xaml.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Tagger.Wpf
{
    using System;
    using System.Windows;
    using System.Windows.Input;

    using Tagger.Properties;
    using Tagger.ViewModels;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class TaggerSettingsWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaggerSettingsWindow"/> class.
        /// </summary>
        public TaggerSettingsWindow()
        {
            InitializeComponent();

            // Initialize data contexts
            var globalSettingsViewModel = new GlobalSettingsViewModel();
            var tagViewModel = new HotkeyViewModel(this.TagHotkeyControl);
            var appearanceViewModel = new HotkeyViewModel(this.SettingsHotkeyControl);
            var trayIconViewModel = new TrayIconViewModel();
            this.TrayIconControl.DataContext = trayIconViewModel;
            this.DataContext = globalSettingsViewModel;

            // Restore previous settings state
            tagViewModel.ModifierKeys = (ModifierKeys)Settings.Default.TagHotkey_Modifiers;
            tagViewModel.Key = (Key)Settings.Default.TagHotkey_Keys;
            appearanceViewModel.ModifierKeys = (ModifierKeys)Settings.Default.AppearanceHotkey_Modifiers;
            appearanceViewModel.Key = (Key)Settings.Default.AppearanceHotkey_Keys;

            // Restore registration state
            tagViewModel.RegisterHotkey();
            appearanceViewModel.RegisterHotkey();

            // Do not tag global settings window
            this.Loaded += delegate { RegistrationManager.RegisterException(this); };

            // Exit application on window close
            this.Closed += delegate { Application.Current.Shutdown(); };

            // Save settings on program deactivation (app exit included)
            Application.Current.Deactivated += delegate
            {
                Settings.Default.TagHotkey_Modifiers = (int)tagViewModel.ModifierKeys;
                Settings.Default.TagHotkey_Keys = (int)tagViewModel.Key;
                Settings.Default.AppearanceHotkey_Modifiers = (int)appearanceViewModel.ModifierKeys;
                Settings.Default.AppearanceHotkey_Keys = (int)appearanceViewModel.Key;
                Settings.Default.Save();
            };

            // Dispose all on exit
            Application.Current.Exit += delegate
            {
                tagViewModel.Dispose();
                appearanceViewModel.Dispose();
                trayIconViewModel.Dispose();
                globalSettingsViewModel.Dispose();
            };

            // Pre-compiling JIT code to speadup first tag appearance
            var context = new TagContext();
            context.Dispose();
        }

        /// <summary>
        /// Handler for window state changed event
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
                this.ShowInTaskbar = false;
            }
        }
    }
}