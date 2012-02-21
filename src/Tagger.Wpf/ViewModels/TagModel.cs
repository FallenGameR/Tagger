using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.Prism;
using Utils.Extensions;
using System.Windows.Media;
using Microsoft.Practices.Prism.Commands;
using Utils.Diagnostics;

namespace Tagger.ViewModels
{
    /// <summary>
    /// View model for tag window
    /// </summary>
    public class TagModel: ViewModelBase
    {
        #region Fields

        private int m_Top;
        private int m_Right;
        private string m_Text;
        private Color m_Color;
        private FontFamily m_FontFamily;
        private double m_FontSize;
        private Color m_FontColor;

        #endregion

        /// <summary>
        /// Initializes new instance of tag view model
        /// </summary>
        /// <param name="context">Tag context that this settings view model belongs to</param>
        public TagModel(TagContext context)
        {
            Check.Require(context != null, "Context must not be null");

            this.TagContext = context;
            this.Color = Colors.Green;

            this.ToggleSettingsCommand = new DelegateCommand<object>(this.ToggleSettings, this.CanToggleSettings);
        }

        #region Validation

        /// <summary>
        /// Validation for Top property value
        /// </summary>
        private void Validate_Top()
        {
            Validate(true, "Is always valid");
        }

        /// <summary>
        /// Validation for Right property value
        /// </summary>
        private void Validate_Right()
        {
            Validate(true, "Is always valid");
        }

        /// <summary>
        /// Validation for Text property value
        /// </summary>
        private void Validate_Text()
        {
            Validate(true, "Is always valid");
        }

        /// <summary>
        /// Validation for Color property value
        /// </summary>
        private void Validate_Color()
        {
            Validate(true, "Is always valid");
        }

        /// <summary>
        /// Validation for FontFamily property value
        /// </summary>
        private void Validate_FontFamily()
        {
            Validate(true, "Is always valid");
        }

        /// <summary>
        /// Validation for FontSize property value
        /// </summary>
        private void Validate_FontSize()
        {
            Validate(true, "Is always valid");
        }

        /// <summary>
        /// Validation for FontColor property value
        /// </summary>
        private void Validate_FontColor()
        {
            Validate(true, "Is always valid");
        }

        #endregion

        #region Properties

        /// <summary>
        /// Tag context 
        /// </summary>
        public TagContext TagContext { get; private set; }

        /// <summary>
        /// Top coordinate of tag window
        /// </summary>
        public int Top
        {
            get { return m_Top; }
            set { m_Top = value; OnPropertyChanged(this.Property(() => Top)); }
        }

        /// <summary>
        /// Right coordinate of tag window
        /// </summary>
        public int Right
        {
            get { return m_Right; }
            set { m_Right = value; OnPropertyChanged(this.Property(() => Right)); }
        }

        /// <summary>
        /// Tag text
        /// </summary>
        /// <remarks>
        /// Defines width and height of window
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

        #endregion

        #region Command - ToggleSettings

        /// <summary>
        /// Shows or hides settings window that is associated with the tag
        /// </summary>
        public DelegateCommand<object> ToggleSettingsCommand { get; private set; }

        /// <summary>
        /// ToggleSettings command handler
        /// </summary>
        private void ToggleSettings(object parameter)
        {
            this.TagContext.SettingsWindow.ToggleVisibility();
        }

        /// <summary>
        /// Test that verifies if ToggleSettings command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanToggleSettings(object parameter)
        {
            return true;
        }

        #endregion
    }
}
