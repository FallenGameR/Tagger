using System.Windows.Controls;
using Tagger.ViewModels;

namespace Tagger.Wpf.Views
{
    /// <summary>
    /// Interaction logic for TrayIconControl.xaml
    /// </summary>
    public partial class TrayIconControl : UserControl
    {
        public TrayIconControl()
        {
            InitializeComponent();
            this.DataContext = new TrayIconViewModel();

            // Existing issues with notify icon:
            // - hidden with the main apliction - http://wpfcontrib.codeplex.com/wikipage?title=NotifyIcon&referringTitle=Home&ProjectName=wpfcontrib
            // - bindings do not work inside the icon - http://wpfcontrib.codeplex.com/workitem/7238
        }
    }
}
