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
    public partial class GlobalSettingsWindow : Window
    {
        public GlobalSettingsWindow()
        {
            // TODO: Check windows OS edition
            InitializeComponent();

            this.Loaded += delegate { RegistrationManager.RegisterException(this); };
            this.Closed += delegate { App.Current.Shutdown(); };

            var tagViewModel = new HotkeyViewModel(this.TagHotkeyControl);
            var appearanceViewModel = new HotkeyViewModel(this.SettingsHotkeyControl);
            var trayIconViewModel = new TrayIconViewModel();
            this.TrayIconControl.DataContext = trayIconViewModel;

            // Restore previous settings state
            tagViewModel.ModifierKeys = (ModifierKeys)Settings.Default.TagHotkey_Modifiers;
            tagViewModel.Key = (Key)Settings.Default.TagHotkey_Keys;
            appearanceViewModel.ModifierKeys = (ModifierKeys)Settings.Default.SettingsHotkey_Modifiers;
            appearanceViewModel.Key = (Key)Settings.Default.SettingsHotkey_Keys;

            // Restore registration state
            tagViewModel.RegisterHotkey();
            appearanceViewModel.RegisterHotkey();

            // Save settings on program deactivation (app exit included)
            App.Current.Deactivated += delegate
            {
                Settings.Default.TagHotkey_Modifiers = (int)tagViewModel.ModifierKeys;
                Settings.Default.TagHotkey_Keys = (int)tagViewModel.Key;
                Settings.Default.SettingsHotkey_Modifiers = (int)appearanceViewModel.ModifierKeys;
                Settings.Default.SettingsHotkey_Keys = (int)appearanceViewModel.Key;
                Settings.Default.Save();
            };

            // Dispose on exit
            App.Current.Exit += delegate
            {
                tagViewModel.Dispose();
                appearanceViewModel.Dispose();
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
