using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
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
        /// Flags that tracks if control was fully initialized
        /// </summary>
        private bool IsFullyInitialized;

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

            this.IsFullyInitialized = false;
            this.IsVisibleChanged += ColorSelectionControl_IsVisibleChanged;
        }

        /// <summary>
        /// Change color picker to advanced mode on control load
        /// </summary>
        /// <remarks>
        /// Control is needed to be loaded to use traverse its visual tree.
        /// Control name is taken from ExtendedWPFToolkit sources of ColorPicker control.
        /// </remarks>
        void ColorSelectionControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsFullyInitialized)
            {
                return;
            }

            if (this.Visibility == Visibility.Visible)
            {
                this.IsFullyInitialized = true;
            }

            var toggleColorModeButtons = (Action)delegate
            {
                Func<DependencyObject, string, bool> name = (d, controlname) =>
                    ((string)d.GetValue(Control.NameProperty)) == controlname;

                var toggleButtons =
                    from visual in this.colorPicker.GetVisualTreeChildren()
                    where name(visual, "PART_ColorPickerPalettePopup")
                    from logical in visual.GetLogicalTreeChildren().OfType<DependencyObject>()
                    where name(logical, "_colorMode")
                    select logical as ToggleButton;

                toggleButtons.Action(b => b.IsChecked = true);
            };

            this.Dispatcher.BeginInvoke(toggleColorModeButtons, DispatcherPriority.Loaded);
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
