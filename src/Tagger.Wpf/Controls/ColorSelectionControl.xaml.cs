using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

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
            typeof(ColorSelectionControl));

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
        }

        /// <summary>
        /// Color that is bound to the control
        /// </summary>
        public Color Color
        {
            get { return (Color)this.GetValue(ColorProperty); }
            set { this.SetValue(ColorProperty, value); }
        }
    }
}
