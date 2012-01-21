using System.Windows.Controls;
using System;

namespace Tagger.Wpf.Views
{
    /// <summary>
    /// Interaction logic for HookControl.xaml
    /// </summary>
    public partial class HookControl : UserControl
    {
        public HookControl()
        {
            InitializeComponent();

            DataContext = new HookViewModel();
            App.Current.Exit += delegate { ((IDisposable)DataContext).Dispose(); };
        }
    }
}
