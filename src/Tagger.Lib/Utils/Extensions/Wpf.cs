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

        /// <summary>
        /// Get all visual tree children for an element
        /// </summary>
        /// <param name="root">Root element</param>
        /// <returns>All visual children flattened</returns>
        /// <remarks>Useful for finding PART_ element of control template</remarks>
        public static IEnumerable<DependencyObject> GetVisualTreeChildren(this DependencyObject root)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                yield return child;

                foreach (var grandchild in GetVisualTreeChildren(child))
                {
                    yield return grandchild;
                }
            }
        }

        /// <summary>
        /// Get all  logical tree children for an element
        /// </summary>
        /// <param name="root">Root element</param>
        /// <returns>All logical children flattened</returns>
        /// <remarks>Useful for finding control inside already found PART_ element</remarks>
        public static IEnumerable<object> GetLogicalTreeChildren(this DependencyObject root)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(root))
            {
                yield return child;

                var dependencyObjectChild = child as DependencyObject;
                if (dependencyObjectChild != null)
                {
                    foreach (var grandchild in GetLogicalTreeChildren(dependencyObjectChild))
                    {
                        yield return grandchild;
                    }
                }
            }
        }
    }
}
