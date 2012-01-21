using System.Windows;

namespace Tagger.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: Window
    {
        public MainWindow()
        {
            // TODO: Check windows OS edition
            InitializeComponent();
            this.Closed += delegate { App.Current.Shutdown(); };
        }
    }
}
