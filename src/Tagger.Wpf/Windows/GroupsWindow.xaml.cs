using System.Windows;

namespace Tagger.Windows
{
    /// <summary>
    /// Interaction logic for GroupsWindow.xaml
    /// </summary>
    public partial class GroupsWindow : Window
    {
        public GroupsWindow()
        {
            InitializeComponent();
            Glass.Enable(this);
        }
    }
}
