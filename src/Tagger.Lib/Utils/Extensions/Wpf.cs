using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Utils.Extensions
{
    public static class Wpf
    {
        /// <summary>
        /// Get top left corner location of a UI element
        /// </summary>
        /// <param name="element">Element which location is queried</param>
        /// <returns>Point coordinates of top left corner of the UI element</returns>
        public static Point GetLocation(this UIElement element)
        {
            return element.TranslatePoint(
                new Point(0, 0),
                element.GetTopLevelElement());
        }

        /// <summary>
        /// Get top level UI element object starting from seed
        /// </summary>
        /// <param name="seed">Start point of the lookup</param>
        /// <returns>The top UI element object (Window or Page)</returns>
        public static UIElement GetTopLevelElement(this UIElement seed)
        {
            var parents = new List<DependencyObject>();

            DependencyObject current = seed;
            while (true)
            {
                current = VisualTreeHelper.GetParent(current);
                if (current == null) { break; }
                parents.Add(current);
            }

            return (UIElement)parents.LastOrDefault(depObj => depObj is UIElement) ?? seed;
        }
    }
}
