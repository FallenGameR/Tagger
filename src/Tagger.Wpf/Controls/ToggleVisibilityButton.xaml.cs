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
        private bool isToggled;

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
            new PropertyMetadata("Untoggled", TextChangedCallback));

        /// <summary>
        /// Button text used for toggled state
        /// </summary>
        public static readonly DependencyProperty ToggledTextProperty = DependencyProperty.Register(
            "ToggledText",
            typeof(string),
            typeof(ToggleVisibilityButton),
            new PropertyMetadata("Untoggled", TextChangedCallback));

        private static void TextChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var toggleVisibilityButton = (ToggleVisibilityButton)d;
            toggleVisibilityButton.UpdateState();
        }

        public ToggleVisibilityButton()
        {
            InitializeComponent();

            this.isToggled = false;
            this.UpdateState();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.isToggled = !this.isToggled;
            this.UpdateState();
        }

        private void UpdateState()
        {
            if (this.isToggled)
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
