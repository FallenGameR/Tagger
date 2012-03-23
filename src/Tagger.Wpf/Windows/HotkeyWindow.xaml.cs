using System;
using System.Windows;
using System.Windows.Input;
using Tagger.Properties;
using Tagger.ViewModels;

namespace Tagger.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class HotkeyWindow : Window
    {
        public HotkeyWindow()
        {
            // TODO: Check windows OS edition
            InitializeComponent();

            this.Loaded += delegate { RegistrationManager.RegisterException(this); };
            this.Closed += delegate { App.Current.Shutdown(); };

            var tagViewModel = new HotkeyViewModel(this.TagHotkeyControl);
            var settingsViewModel = new HotkeyViewModel(this.SettingsHotkeyControl);
            var trayIconViewModel = new TrayIconViewModel();
            this.TrayIconControl.DataContext = trayIconViewModel;

            // Restore previous settings state
            tagViewModel.ModifierKeys = (ModifierKeys)Settings.Default.TagHotkey_Modifiers;
            tagViewModel.Key = (Key)Settings.Default.TagHotkey_Keys;
            settingsViewModel.ModifierKeys = (ModifierKeys)Settings.Default.SettingsHotkey_Modifiers;
            settingsViewModel.Key = (Key)Settings.Default.SettingsHotkey_Keys;

            // Restore registration state
            tagViewModel.RegisterHotkey();
            settingsViewModel.RegisterHotkey();

            // Save settings on program deactivation (app exit included)
            App.Current.Deactivated += delegate
            {
                Settings.Default.TagHotkey_Modifiers = (int)tagViewModel.ModifierKeys;
                Settings.Default.TagHotkey_Keys = (int)tagViewModel.Key;
                Settings.Default.SettingsHotkey_Modifiers = (int)settingsViewModel.ModifierKeys;
                Settings.Default.SettingsHotkey_Keys = (int)settingsViewModel.Key;
                Settings.Default.Save();
            };

            // Dispose on exit
            App.Current.Exit += delegate
            {
                tagViewModel.Dispose();
                settingsViewModel.Dispose();
                trayIconViewModel.Dispose();
            };
        }

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
