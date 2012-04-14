using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Utils.Extensions;

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
            this.Loaded += this.ColorSelectionControl_Loaded;
        }

        /// <summary>
        /// Change color picker to advanced mode on control load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// Control is needed to be loaded to use traverse its visual tree.
        /// Control name is taken from ExtendedWPFToolkit sources of ColorPicker control.
        /// </remarks>
        void ColorSelectionControl_Loaded(object sender, RoutedEventArgs e)
        {
            Func<DependencyObject,string,bool> name = (d, controlname) =>
                ((string)d.GetValue(Control.NameProperty)) == controlname;

            var control = this.colorPicker.GetVisualTreeChildren().FirstOrDefault(d => name(d, "PART_ColorPickerPalettePopup"));
            if (control == null) { return; }

            var toggle = control.GetLogicalTreeChildren().OfType<DependencyObject>().FirstOrDefault(d => name(d, "_colorMode")) as ToggleButton;
            if (toggle == null) { return; }

            toggle.IsChecked = true;
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
