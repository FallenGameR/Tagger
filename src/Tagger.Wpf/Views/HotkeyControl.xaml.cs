using System.Windows.Controls;
using System.Windows;
using System;

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

        /// <summary>
        /// Dependency property for Handler property
        /// </summary>
        public static readonly DependencyProperty HandlerProperty = DependencyProperty.Register(
            "Handler",
            typeof(Action),
            typeof(HotkeyControl),
            new UIPropertyMetadata(null));

        /// <summary>
        /// Global hotkey handler delegate
        /// </summary>
        public Action Handler
        {
            get { return (Action)this.GetValue(HandlerProperty); }
            set { this.SetValue(HandlerProperty, value); }
        }
    }
}
