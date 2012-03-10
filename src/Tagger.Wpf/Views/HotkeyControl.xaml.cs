namespace Tagger.Wpf.Views
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for HotkeyControl.xaml
    /// </summary>
    public partial class HotkeyControl : UserControl
    {
        /// <summary>
        /// Dependency propery for Purpose property
        /// </summary>
        public static readonly DependencyProperty PurposeProperty = DependencyProperty.Register(
            "Purpose",
            typeof(string),
            typeof(HotkeyControl),
            new UIPropertyMetadata("Shortcut key"));

        /// <summary>
        /// Dependency property for Handler property
        /// </summary>
        public static readonly DependencyProperty HandlerProperty = DependencyProperty.Register(
            "Handler",
            typeof(Action),
            typeof(HotkeyControl),
            new UIPropertyMetadata(null));

        /// <summary>
        /// Initializes a new instance of the HotkeyControl class
        /// </summary>
        public HotkeyControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets purpose of the hotkey, what it is used for
        /// </summary>
        public string Purpose
        {
            get { return (string)this.GetValue(PurposeProperty); }
            set { this.SetValue(PurposeProperty, value); }
        }

        /// <summary>
        /// Gets or sets global hotkey handler delegate
        /// </summary>
        public Action Handler
        {
            get { return (Action)this.GetValue(HandlerProperty); }
            set { this.SetValue(HandlerProperty, value); }
        }
    }
}
