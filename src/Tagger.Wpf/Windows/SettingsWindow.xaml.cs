using System;
using System.Windows;
using Tagger.ViewModels;
using Utils.Extensions;

namespace Tagger.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        /// <summary>
        /// Constructor that is used by the studio designer
        /// </summary>
        public SettingsWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes and shows new instance of settings window 
        /// </summary>
        /// <param name="host">Host window the corresponding tag belong to</param>
        /// <param name="viewModel">View model with all fields needed for settings window render</param>
        public SettingsWindow(IntPtr host, SettingsModel viewModel)
            : this()
        {
            this.DataContext = viewModel;
            this.SetOwner(host);
            this.Show();
        }
    }
}
