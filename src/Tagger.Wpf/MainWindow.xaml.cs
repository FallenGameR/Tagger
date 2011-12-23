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
            // Check windows OS edition

            InitializeComponent();

            // Prepopulate compiled assebly cache
            //Utils.PreloadAccessibilityAssembly();

            this.Closed += delegate { App.Current.Shutdown(); };
        }
    }
}
