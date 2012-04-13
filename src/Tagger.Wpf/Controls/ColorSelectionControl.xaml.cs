using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Reflection;

namespace Tagger.Controls
{
    /// <summary>
    /// Interaction logic for ColorSelectionControl.xaml
    /// </summary>
    public partial class ColorSelectionControl : UserControl
    {
        /// <summary>
        /// Color that is bound to the control
        /// </summary>
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            "Color",
            typeof(Color),
            typeof(ColorSelectionControl),
            new PropertyMetadata(Colors.Black));

        /// <summary>
        /// Color names resource
        /// </summary>
        /// <remarks>
        /// Not moved to XAML since in C# it's way more readable to call Select from LINQ
        /// </remarks>
        public static string[] ColorNames = typeof(Colors).GetProperties().Select(pi => pi.Name).ToArray();

        public ColorSelectionControl()
        {
            InitializeComponent();

            //this.colorPicker.Template find ToggleButton _colorMode and set IsChecked = true
        }

        /// <summary>
        /// Color that is bound to the control
        /// </summary>
        public Color Color
        {
            get { return (Color)this.GetValue(ColorProperty); }
            set { this.SetValue(ColorProperty, value); }
        }

        /// <summary>
        /// Pick a random color
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Color = ColorRandom.Next();
        }
    }
}
