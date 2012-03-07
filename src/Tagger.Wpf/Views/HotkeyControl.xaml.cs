using System.Windows.Controls;
using Tagger.ViewModels;

namespace Tagger.Wpf.Views
{
    /// <summary>
    /// Interaction logic for HotkeyControl.xaml
    /// </summary>
    public partial class HotkeyControl : UserControl
    {
        public HotkeyControl()
        {
            InitializeComponent();
            new HotkeyViewModel().BindToView(this); 
        }
    }
}
