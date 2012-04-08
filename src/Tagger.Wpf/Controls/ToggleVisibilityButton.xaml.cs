using System.Windows;
using System.Windows.Controls;
using Tagger.ViewModels;

namespace Tagger.Controls
{
    /// <summary>
    /// Interaction logic for ToggleVisibilityButton.xaml
    /// </summary>
    public partial class ToggleVisibilityButton : UserControl
    {
        public ToggleVisibilityButton()
        {
            InitializeComponent();
            this.DataContext = new ToggleVisibilityButtonViewModel();
        }

        public static readonly DependencyProperty UntoggledTextProperty = DependencyProperty.Register(
            "UntoggledText",
            typeof(string),
            typeof(ToggleVisibilityButton));

        public static readonly DependencyProperty ToggledTextProperty = DependencyProperty.Register(
            "ToggledText",
            typeof(string),
            typeof(ToggleVisibilityButton));

        public string UntoggledText
        {
            get { return (string)this.GetValue(UntoggledTextProperty); }
            set { this.SetValue(UntoggledTextProperty, value); }
        }

        public string ToggledText
        {
            get { return (string)this.GetValue(ToggledTextProperty); }
            set { this.SetValue(ToggledTextProperty, value); }
        }
    }

}
