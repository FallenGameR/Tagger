using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;

namespace Utils.Prism
{
    /// <summary>
    /// Converter for colors that are rendered as text
    /// </summary>
    /// <remarks>
    /// Converter tries its best to keep text in human readable form
    /// </remarks>
    public class ColorStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var query =
                from property in typeof(Colors).GetProperties()
                let color = property.GetValue(null, null)
                where color.Equals(value)
                select property.Name;

            return query.FirstOrDefault() ?? value.ToString();
        }

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
