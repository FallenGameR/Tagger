using System.Windows;
using ManagedWinapi.Accessibility;
using System;
using Tagger.WinAPI.WaitChainTraversal;
using Tagger.Lib;

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

            DataContext = new HookViewModel
            {
                ProcessId = 3704,
            };
        }



    }
}
