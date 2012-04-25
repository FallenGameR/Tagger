//-----------------------------------------------------------------------
// <copyright file="ColorStringConverter.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Utils.Prism
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using System.Windows.Media;

    /// <summary>
    /// Converter for colors that are rendered as text
    /// </summary>
    /// <remarks>
    /// Converter tries its best to keep text in human readable form
    /// </remarks>
    public class ColorStringConverter : IValueConverter
    {
        /// <summary>
        /// Convert from color to string
        /// </summary>
        /// <param name="value">Color value to convert.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>
        /// String representation for the color. Human readable name would be used if known.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var query =
                from property in typeof(Colors).GetProperties()
                let color = property.GetValue(null, null)
                where color.Equals(value)
                select property.Name;

            return query.FirstOrDefault() ?? value.ToString();
        }

        /// <summary>
        /// Convert from string to color
        /// </summary>
        /// <param name="value">String value to convert.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>
        /// Color that is encoded in the string.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return ColorConverter.ConvertFromString((string)value);
            }
            catch (FormatException)
            {
                return Colors.White;
            }
        }
    }
}
