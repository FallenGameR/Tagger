// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TagViewModel.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Tagger
{
    using System.Windows.Media;

    using Microsoft.Practices.Prism.Commands;

    using Tagger.Properties;

    using Utils.Extensions;
    using Utils.Prism;

    /// <summary>
    /// View model needed to render tag window
    /// </summary>
    public class TagViewModel : ViewModelBase
    {
        #region Fields

        /// <summary>
        /// The tag text.
        /// </summary>
        private string text;

        /// <summary>
        /// The tag color.
        /// </summary>
        private Color color;

        /// <summary>
        /// The tag font color.
        /// </summary>
        private Color fontColor;

        /// <summary>
        /// The tag font family.
        /// </summary>
        private FontFamily fontFamily;

        /// <summary>
        /// The tag font size.
        /// </summary>
        private double fontSize;

        /// <summary>
        /// The tag offset right.
        /// </summary>
        private int offsetRight;

        /// <summary>
        /// The tag offset top.
        /// </summary>
        private int offsetTop;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TagViewModel"/> class. 
        /// </summary>
        public TagViewModel()
        {
            // Initialize commands (settings related commands are just stubs - we'll inject them latter)
            this.SaveAsDefaultCommand = new DelegateCommand<object>(o => this.SaveAsDefault());
            this.LoadFromDefaultCommand = new DelegateCommand<object>(o => this.LoadFromDefault());
            this.ToggleSettingsCommand = new DelegateCommand<object>(delegate { });
            this.HideSettingsCommand = new DelegateCommand<object>(delegate { });
            this.KillTagCommand = new DelegateCommand<object>(o => this.KillTag());

            // Load properties from default values
            this.LoadFromDefaultCommand.Execute(null);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets tag text
        /// </summary>
        /// <remarks>
        /// Defines width and height of tag window
        /// </remarks>
        public string Text
        {
            get
            {
                return this.text;
            }

            set
            {
                this.text = value;
                this.OnPropertyChanged(this.Property(() => this.Text));
            }
        }

        /// <summary>
        /// Gets or sets color used for tag
        /// </summary>
        public Color Color
        {
            get
            {
                return this.color;
            }

            set
            {
                this.color = value;
                this.OnPropertyChanged(this.Property(() => this.Color));
            }
        }

        /// <summary>
        /// Gets or sets color of the font used to render text
        /// </summary>
        public Color FontColor
        {
            get
            {
                return this.fontColor;
            }

            set
            {
                this.fontColor = value;
                this.OnPropertyChanged(this.Property(() => this.FontColor));
            }
        }

        /// <summary>
        /// Gets or sets font family used for rendering tag text
        /// </summary>
        public FontFamily FontFamily
        {
            get
            {
                return this.fontFamily;
            }

            set
            {
                this.fontFamily = value;
                this.OnPropertyChanged(this.Property(() => this.FontFamily));
            }
        }

        /// <summary>
        /// Gets or sets size of the font used to render tag text
        /// </summary>
        public double FontSize
        {
            get
            {
                return this.fontSize;
            }

            set
            {
                this.fontSize = value;
                this.OnPropertyChanged(this.Property(() => this.FontSize));
            }
        }

        /// <summary>
        /// Gets or sets right offset coordinate of tag window
        /// </summary>
        public int OffsetRight
        {
            get
            {
                return this.offsetRight;
            }

            set
            {
                this.offsetRight = value;
                this.OnPropertyChanged(this.Property(() => this.OffsetRight));
            }
        }

        /// <summary>
        /// Gets or sets top offset coordinate of tag window
        /// </summary>
        public int OffsetTop
        {
            get
            {
                return this.offsetTop;
            }

            set
            {
                this.offsetTop = value;
                this.OnPropertyChanged(this.Property(() => this.OffsetTop));
            }
        }

        /// <summary>
        /// Gets hide settings window command
        /// </summary>
        public DelegateCommand<object> HideSettingsCommand { get; internal set; }

        /// <summary>
        /// Gets command to kill current tag
        /// </summary>
        public DelegateCommand<object> KillTagCommand { get; private set; }

        /// <summary>
        /// Gets command to load current settings from default values
        /// </summary>
        public DelegateCommand<object> LoadFromDefaultCommand { get; private set; }

        /// <summary>
        /// Gets command to save current settings as default for all new tags
        /// </summary>
        public DelegateCommand<object> SaveAsDefaultCommand { get; private set; }

        /// <summary>
        /// Gets command that shows or hides settings window that is associated with the tag
        /// </summary>
        public DelegateCommand<object> ToggleSettingsCommand { get; internal set; }

        #endregion

        #region Methods

        /// <summary>
        /// Get grouping label for the tag
        /// </summary>
        /// <returns>
        /// Tag label that is used to make tag groups
        /// </returns>
        public TagLabel GetLabel()
        {
            return new TagLabel(this);
        }

        /// <summary>
        /// Kill current tag
        /// </summary>
        private void KillTag()
        {
            RegistrationManager.UnregisterTag(this);
        }

        /// <summary>
        /// Load all render settings from default values stored in program settings
        /// </summary>
        private void LoadFromDefault()
        {
            this.Text = Settings.Default.Tag_Text;
            this.Color = (Color)ColorConverter.ConvertFromString(Settings.Default.Tag_Color);
            this.FontFamily = new FontFamily(Settings.Default.Tag_FontFamily);
            this.FontSize = Settings.Default.Tag_FontSize;
            this.FontColor = (Color)ColorConverter.ConvertFromString(Settings.Default.Tag_FontColor);
            this.OffsetTop = Settings.Default.Tag_OffsetTop;
            this.OffsetRight = Settings.Default.Tag_OffsetRight;
        }

        /// <summary>
        /// Save all render settings as default that would be used for every new tag
        /// </summary>
        private void SaveAsDefault()
        {
            Settings.Default.Tag_Text = this.Text;
            Settings.Default.Tag_Color = this.Color.ToString();
            Settings.Default.Tag_FontFamily = this.FontFamily.Source;
            Settings.Default.Tag_FontSize = this.FontSize;
            Settings.Default.Tag_FontColor = this.FontColor.ToString();
            Settings.Default.Tag_OffsetTop = this.OffsetTop;
            Settings.Default.Tag_OffsetRight = this.OffsetRight;
            Settings.Default.Save();
        }

        #endregion
    }
}