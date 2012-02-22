using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Utils.Extensions;
using Utils.Prism;

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
            this.Text = "File browser";
            this.Color = Colors.Green;
            this.FontFamily = new FontFamily("Segoe UI");
            this.FontSize = 50;
            this.FontColor = Colors.Black;
            this.OffsetTop = 0;
            this.OffsetRight = 0;
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
    }
}
