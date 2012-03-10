using System.Windows.Controls;
using System.Windows;

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
        }

        /// <summary>
        /// Dependency propery for Purpose property
        /// </summary>
        public static readonly DependencyProperty PurposeProperty = DependencyProperty.Register(
            "Purpose",
            typeof(string),
            typeof(HotkeyControl),
            new UIPropertyMetadata("Shortcut key"));

        /// <summary>
        /// Purpose of the hotkey, what it is used for
        /// </summary>
        public string Purpose
        {
            get { return (string)this.GetValue(PurposeProperty); }
            set { this.SetValue(PurposeProperty, value); }
        }
    }
}
