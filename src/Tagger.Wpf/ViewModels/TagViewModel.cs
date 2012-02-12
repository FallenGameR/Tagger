using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.Prism;
using Utils.Extensions;
using System.Windows.Media;
using Microsoft.Practices.Prism.Commands;

namespace Tagger.Wpf.ViewModels
{
    public class TagViewModel: ViewModelBase
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

        public TagViewModel()
        {
            this.Color = Colors.Green;

            this.AttachTagCommand = new DelegateCommand<object>(AttachTag, CanAttachTag);
            this.DetachTagCommand = new DelegateCommand<object>(DetachTag, CanDetachTag);
            this.ShowSettingsCommand = new DelegateCommand<object>(ShowSettings, CanShowSettings);
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

        #region Command - AttachTag

        /// <summary>
        /// Attaches tag window to host
        /// </summary>
        public DelegateCommand<object> AttachTagCommand { get; private set; }

        /// <summary>
        /// AttachTag command handler
        /// </summary>
        private void AttachTag(object parameter)
        {
        }

        /// <summary>
        /// Test that verifies if AttachTag command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanAttachTag(object parameter)
        {
            return true;
        }

        #endregion

        #region Command - DetachTag

        /// <summary>
        /// Detaches tag window from the host
        /// </summary>
        public DelegateCommand<object> DetachTagCommand { get; private set; }

        /// <summary>
        /// DetachTag command handler
        /// </summary>
        private void DetachTag(object parameter)
        {
        }

        /// <summary>
        /// Test that verifies if DetachTag command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanDetachTag(object parameter)
        {
            return true;
        }

        #endregion

        #region Command - ShowSettings

        /// <summary>
        /// Shows settings window that are associated with the tag
        /// </summary>
        public DelegateCommand<object> ShowSettingsCommand { get; private set; }

        /// <summary>
        /// ShowSettings command handler
        /// </summary>
        private void ShowSettings(object parameter)
        {
        }

        /// <summary>
        /// Test that verifies if ShowSettings command can be invoked
        /// </summary>
        /// <returns>true if command could be invoked</returns>
        private bool CanShowSettings(object parameter)
        {
            return true;
        }

        #endregion
    }
}
