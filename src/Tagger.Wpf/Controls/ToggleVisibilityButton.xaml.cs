using System.Windows;
using System.Windows.Controls;
using Tagger.ViewModels;
using Microsoft.Practices.Prism.Commands;

namespace Tagger.Controls
{
    /// <summary>
    /// Interaction logic for ToggleVisibilityButton.xaml
    /// </summary>
    public partial class ToggleVisibilityButton : UserControl
    {
        /// <summary>
        /// Flags that shows if button is toggled
        /// </summary>
        public static readonly DependencyProperty IsToggledProperty = DependencyProperty.Register(
            "IsToggled",
            typeof(bool),
            typeof(ToggleVisibilityButton),
            new PropertyMetadata(false, UpdateStateNeededCallback));

        /// <summary>
        /// Button text
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(ToggleVisibilityButton));

        /// <summary>
        /// Visibility state for associated control
        /// </summary>
        public static readonly DependencyProperty VisibilityStateProperty = DependencyProperty.Register(
            "VisibilityState",
            typeof(Visibility),
            typeof(ToggleVisibilityButton));

        /// <summary>
        /// Button text used for untoggled state
        /// </summary>
        public static readonly DependencyProperty UntoggledTextProperty = DependencyProperty.Register(
            "UntoggledText",
            typeof(string),
            typeof(ToggleVisibilityButton),
            new PropertyMetadata("Untoggled", UpdateStateNeededCallback));

        /// <summary>
        /// Button text used for toggled state
        /// </summary>
        public static readonly DependencyProperty ToggledTextProperty = DependencyProperty.Register(
            "ToggledText",
            typeof(string),
            typeof(ToggleVisibilityButton),
            new PropertyMetadata("Untoggled", UpdateStateNeededCallback));

        public ToggleVisibilityButton()
        {
            InitializeComponent();
            this.UpdateState();
        }

        private static void UpdateStateNeededCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var toggleVisibilityButton = (ToggleVisibilityButton)d;
            toggleVisibilityButton.UpdateState();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.IsToggled = !this.IsToggled;
        }

        private void UpdateState()
        {
            if (this.IsToggled)
            {
                this.Text = this.ToggledText;
                this.VisibilityState = Visibility.Visible;
            }
            else
            {
                this.Text = this.UntoggledText;
                this.VisibilityState = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Flags that shows if button is toggled
        /// </summary>
        public bool IsToggled
        {
            get { return (bool)this.GetValue(IsToggledProperty); }
            set { this.SetValue(IsToggledProperty, value); }
        }

        /// <summary>
        /// Button text
        /// </summary>
        private string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Visibility state for associated control
        /// </summary>
        public Visibility VisibilityState
        {
            get { return (Visibility)this.GetValue(VisibilityStateProperty); }
            set { this.SetValue(VisibilityStateProperty, value); }
        }

        /// <summary>
        /// Button text used for untoggled state
        /// </summary>
        public string UntoggledText
        {
            get { return (string)this.GetValue(UntoggledTextProperty); }
            set { this.SetValue(UntoggledTextProperty, value); }
        }

        /// <summary>
        /// Button text used for toggled state
        /// </summary>
        public string ToggledText
        {
            get { return (string)this.GetValue(ToggledTextProperty); }
            set { this.SetValue(ToggledTextProperty, value); }
        }
    }
}
