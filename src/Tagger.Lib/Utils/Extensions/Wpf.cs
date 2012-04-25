//-----------------------------------------------------------------------
// <copyright file="Wpf.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Utils.Extensions
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// Extension class for WPF operations on trees
    /// </summary>
    public static class Wpf
    {
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
