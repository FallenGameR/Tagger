// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ToggleVisibilityButton.xaml.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Tagger.Controls
{
    using System.Windows;
    using System.Windows.Controls;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleVisibilityButton"/> class.
        /// </summary>
        public ToggleVisibilityButton()
        {
            this.InitializeComponent();
            this.UpdateState();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the button is toggled
        /// </summary>
        public bool IsToggled
        {
            get { return (bool)this.GetValue(IsToggledProperty); }
            set { this.SetValue(IsToggledProperty, value); }
        }

        /// <summary>
        /// Gets or sets button text used for toggled state
        /// </summary>
        public string ToggledText
        {
            get { return (string)this.GetValue(ToggledTextProperty); } 
            set { this.SetValue(ToggledTextProperty, value); }
        }

        /// <summary>
        /// Gets or sets button text used for untoggled state
        /// </summary>
        public string UntoggledText
        {
            get { return (string)this.GetValue(UntoggledTextProperty); } 
            set { this.SetValue(UntoggledTextProperty, value); }
        }

        /// <summary>
        /// Gets or sets visibility state for associated control
        /// </summary>
        public Visibility VisibilityState
        {
            get { return (Visibility)this.GetValue(VisibilityStateProperty); } 
            set { this.SetValue(VisibilityStateProperty, value); }
        }

        /// <summary>
        /// Gets or sets button text
        /// </summary>
        private string Text
        {
            get { return (string)this.GetValue(TextProperty); } 
            set { this.SetValue(TextProperty, value); }
        }

        /// <summary>
        /// The update state needed callback
        /// </summary>
        /// <param name="d">Button object that receives the event.</param>
        /// <param name="e">The parameter is not used.</param>
        private static void UpdateStateNeededCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var toggleVisibilityButton = (ToggleVisibilityButton)d;
            toggleVisibilityButton.UpdateState();
        }

        /// <summary>
        /// Button click handler that toggles state
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.IsToggled = !this.IsToggled;
        }

        /// <summary>
        /// Update state of the button control
        /// </summary>
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
    }
}
