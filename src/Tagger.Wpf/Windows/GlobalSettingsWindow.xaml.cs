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
            globalSettingsViewModel.UseColorRandomization = Settings.Default.GlobalSettings_UseColorRandomization;

            // Restore registration state
            tagViewModel.RegisterHotkey();
            appearanceViewModel.RegisterHotkey();

            // Save settings on program deactivation (app exit included)
            App.Current.Deactivated += delegate
            {
                Settings.Default.TagHotkey_Modifiers = (int)tagViewModel.ModifierKeys;
                Settings.Default.TagHotkey_Keys = (int)tagViewModel.Key;
                Settings.Default.AppearanceHotkey_Modifiers = (int)appearanceViewModel.ModifierKeys;
                Settings.Default.AppearanceHotkey_Keys = (int)appearanceViewModel.Key;
                Settings.Default.GlobalSettings_UseColorRandomization = globalSettingsViewModel.UseColorRandomization;
                Settings.Default.Save();
            };

            // Dispose on exit
            App.Current.Exit += delegate
            {
                tagViewModel.Dispose();
                appearanceViewModel.Dispose();
                trayIconViewModel.Dispose();
                globalSettingsViewModel.Dispose();
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
