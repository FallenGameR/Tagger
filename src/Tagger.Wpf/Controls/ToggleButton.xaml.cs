using System.Windows;
using System.Windows.Controls;
using Utils.Extensions;
using Utils.Prism;

namespace Tagger.Controls
{
    /// <summary>
    /// Interaction logic for ToggleButton.xaml
    /// </summary>
    public partial class ToggleButton : UserControl
    {
        private class ViewModel: ViewModelBase
        {
            private string text;
            private Visibility visibilityState;
            private bool isToggled;
            private string untoggledText;
            private string toggledText;

            public ViewModel()
            {
                this.untoggledText = "Untoggled";
                this.toggledText = "Toggled";
            }

            /// <summary>
            /// Button text displayed
            /// </summary>
            public string Text
            {
                get
                {
                    return this.text;
                }
                private set
                {
                    this.text = value;
                    OnPropertyChanged(this.Property(() => Text));
                }
            }

            /// <summary>
            /// Visibility state for associated control
            /// </summary>
            public Visibility VisibilityState
            {
                get
                {
                    return this.visibilityState;
                }
                private set
                {
                    this.visibilityState = value;
                    OnPropertyChanged(this.Property(() => VisibilityState));
                }
            }


            public bool IsToggled
            {
                get
                {
                    return this.isToggled;
                }
                set
                {
                    this.isToggled = value;

                    if (this.isToggled)
                    {
                        this.Text = this.toggledText;
                        this.VisibilityState = Visibility.Visible;
                    }
                    else
                    {
                        this.Text = this.untoggledText;
                        this.VisibilityState = Visibility.Collapsed;
                    }
                }
            }

        }

        public ToggleButton()
        {
            InitializeComponent();
            this.DataContext = new ViewModel();
        }

        public static readonly DependencyProperty UntoggledTextProperty =
            DependencyProperty.Register("UntoggledText", typeof(string), typeof(ToggleButton));

        public string UntoggledText
        {
            get
            {
                return (string)this.GetValue(UntoggledTextProperty);
            }
            set
            {
                this.SetValue(UntoggledTextProperty, value);
            }
        }


    }

}
