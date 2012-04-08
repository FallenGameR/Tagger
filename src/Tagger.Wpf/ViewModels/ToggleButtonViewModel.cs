namespace Tagger.ViewModels
{
    using System.Windows;
    using Microsoft.Practices.Prism.Commands;
    using Utils.Extensions;
    using Utils.Prism;

    /// <summary>
    /// View model for toggle button
    /// </summary>
    public class ToggleVisibilityButtonViewModel : ViewModelBase
    {
        private string text;
        private Visibility visibilityState;
        private bool isToggled;
        private string untoggledText;
        private string toggledText;

        public ToggleVisibilityButtonViewModel()
        {
            this.isToggled = false;
            this.untoggledText = "Untoggled";
            this.toggledText = "Toggled";

            this.ToggleCommand = new DelegateCommand<object>(this.Toggle);

            this.Update();
        }

        private void Update()
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

        private void Toggle(object sender)
        {
            this.isToggled = !this.isToggled;
            this.Update();
        }

        /// <summary>
        /// Toggle button
        /// </summary>
        public DelegateCommand<object> ToggleCommand { get; private set; }

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

        /// <summary>
        /// Button text used for toggled state
        /// </summary>
        public string ToggledText
        {
            get
            {
                return this.toggledText;
            }
            set
            {
                this.toggledText = value;
                this.Update();
            }
        }

        /// <summary>
        /// Button text used for untoggled state
        /// </summary>
        public string UntoggledText
        {
            get
            {
                return this.untoggledText;
            }
            set
            {
                this.untoggledText = value;
                this.Update();
            }
        }
    }
}
