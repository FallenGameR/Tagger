using Microsoft.Practices.Prism.Commands;
using Tagger.Wpf;
using Utils.Prism;

namespace Tagger.ViewModels
{
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
            this.CloseProgramCommand = new DelegateCommand<object>(delegate { App.Current.Shutdown(); });
        }

        /// <summary>
        /// Show settings command 
        /// </summary>
        public DelegateCommand<object> ShowSettingsCommand { get; private set; }

        /// <summary>
        /// Close program command
        /// </summary>
        public DelegateCommand<object> CloseProgramCommand { get; private set; }
    }
}
