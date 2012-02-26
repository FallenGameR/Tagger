using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Utils.Extensions;
using Utils.Prism;
using Tagger.Properties;
using Microsoft.Practices.Prism.Commands;

namespace Tagger
{
    /// <summary>
    /// View model needed to render tag window
    /// </summary>
    public class TagRender: ViewModelBase
    {
        #region Fields

        private string m_Text;
        private Color m_Color;
        private FontFamily m_FontFamily;
        private double m_FontSize;
        private Color m_FontColor;
        private int m_OffsetTop;
        private int m_OffsetRight;

        #endregion

        /// <summary>
        /// Initializes new instance of TagRender
        /// </summary>
        public TagRender()
        {
            this.LoadFromDefault();
        }

        #region Properties

        /// <summary>
        /// Tag text
        /// </summary>
        /// <remarks>
        /// Defines width and height of tag window
        /// </remarks>
        public string Text
        {
            get { return m_Text; }
            set { m_Text = value; OnPropertyChanged(this.Property(() => Text)); }
        }

        /// <summary>
        /// Color used for tag
        /// </summary>
        public Color Color
        {
            get { return m_Color; }
            set { m_Color = value; OnPropertyChanged(this.Property(() => Color)); }
        }

        /// <summary>
        /// Font family used for rendering tag text
        /// </summary>
        public FontFamily FontFamily
        {
            get { return m_FontFamily; }
            set { m_FontFamily = value; OnPropertyChanged(this.Property(() => FontFamily)); }
        }

        /// <summary>
        /// Size of the font used to render tag text
        /// </summary>
        public double FontSize
        {
            get { return m_FontSize; }
            set { m_FontSize = value; OnPropertyChanged(this.Property(() => FontSize)); }
        }

        /// <summary>
        /// Color of the font used to render text
        /// </summary>
        public Color FontColor
        {
            get { return m_FontColor; }
            set { m_FontColor = value; OnPropertyChanged(this.Property(() => FontColor)); }
        }

        /// <summary>
        /// Top offset coordinate of tag window
        /// </summary>
        public int OffsetTop
        {
            get { return m_OffsetTop; }
            set { m_OffsetTop = value; OnPropertyChanged(this.Property(() => OffsetTop)); }
        }

        /// <summary>
        /// Right offset coordinate of tag window
        /// </summary>
        public int OffsetRight
        {
            get { return m_OffsetRight; }
            set { m_OffsetRight = value; OnPropertyChanged(this.Property(() => OffsetRight)); }
        }

        #endregion

        /// <summary>
        /// Save all render settings as default that would be used for every new tag
        /// </summary>
        public void SaveAsDefault()
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

        /// <summary>
        /// Load all render settings from default values stored in program settings
        /// </summary>
        public void LoadFromDefault()
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
        /// Shows or hides settings window that is associated with the tag
        /// </summary>
        public DelegateCommand<object> ToggleSettingsCommand { get; internal set; }

        /// <summary>
        /// Hide settings window command
        /// </summary>
        public DelegateCommand<object> HideSettingsCommand { get; internal set; }

        /// <summary>
        /// Command to save current settings as default for all new tags
        /// </summary>
        public DelegateCommand<object> SaveAsDefaultCommand { get; internal set; }

        /// <summary>
        /// Command to load current settings from default values
        /// </summary>
        public DelegateCommand<object> LoadFromDefaultCommand { get; internal set; }
    }
}
