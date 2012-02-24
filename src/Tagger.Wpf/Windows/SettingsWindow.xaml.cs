using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;

namespace Tagger.Wpf.Windows
{
    public partial class SettingsWindow : Window
    {
        public static string[] ColorNames = typeof(Colors).GetProperties().Select( pi => pi.Name).ToArray();

        public SettingsWindow()
        {
            InitializeComponent();
        }
    }
}
