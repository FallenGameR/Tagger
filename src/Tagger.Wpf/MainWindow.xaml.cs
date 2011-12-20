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
            // Check windows edition
            InitializeComponent();

            // Prepopulate compiled assebly cache
            //Utils.PreloadAccessibilityAssembly();

            var viewModel = new HookViewModel();
            DataContext = viewModel;
            viewModel.StartWindowedApplicationCommand.Execute(null);

            Closed += delegate { viewModel.Dispose(); };
        }
    }
}
