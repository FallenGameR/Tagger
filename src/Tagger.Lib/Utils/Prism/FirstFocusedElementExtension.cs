//-----------------------------------------------------------------------
// <copyright file="FirstFocusedElementExtension.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Utils.Prism
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media;

    /// <summary>
    /// This markup extension locates the first focusable child and returns it.
    /// It is intended to be used with FocusManager.FocusedElement:
    /// </summary>
    /// <example>
    /// In XAML: Window FocusManager.FocusedElement={ft:FirstFocusedElement} 
    /// </example>
    /// <remarks>
    /// Original was taken from some WPF site.
    /// </remarks>
    public class FirstFocusedElementExtension : MarkupExtension
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FirstFocusedElementExtension"/> class. 
        /// </summary>
        public FirstFocusedElementExtension()
        {
            this.OneTime = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether we should unhook the 
        /// handler after it has set focus to the element the first time
        /// </summary>
        public bool OneTime { get; set; }

        /// <summary>
        /// This method locates the first focusable + visible element we can
        /// change focus to.
        /// </summary>
        /// <param name="serviceProvider">IServiceProvider from XAML</param>
        /// <returns>Focusable Element or null</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // Ignore if in design mode
            if ((bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue)
            {
                return null;
            }

            // Get the IProvideValue interface which gives us access to the target property 
            // and object. MSDN actually calls this interface "internal" but it's necessary
            // here because we need to know what property we are assigning to.
            var pvt = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (pvt != null)
            {
                var fe = pvt.TargetObject as FrameworkElement;
                object targetProperty = pvt.TargetProperty;
                if (fe != null)
                {
                    // If the element isn't loaded yet, then wait for it.
                    if (!fe.IsLoaded)
                    {
                        RoutedEventHandler deferredFocusHookup = null;
                        RoutedEventHandler hookup = deferredFocusHookup;
                        deferredFocusHookup = delegate
                        {
                            // Ignore if the element is now loaded but not
                            // visible -- this happens for things like TabItem.
                            // Instead, we'll wait until the item becomes visible and
                            // then set focus.
                            if (fe.IsVisible == false)
                            {
                                return;
                            }

                            // Look for the first focusable leaf child and set the property
                            IInputElement ie = GetLeafFocusableChild(fe);
                            if (targetProperty is DependencyProperty)
                            {
                                // Specific case where we are setting focused element.
                                // We really need to set this property onto the focus scope, 
                                // so we'll use UIElement.Focus() which will do exactly that.
                                if (targetProperty == FocusManager.FocusedElementProperty)
                                {
                                    ie.Focus();
                                }
                                else
                                {
                                    // Being assigned to some other property - just assign it.
                                    fe.SetValue((DependencyProperty)targetProperty, ie);
                                }
                            }
                            else if (targetProperty is PropertyInfo)
                            {
                                // Simple property assignment through reflection.
                                var pi = (PropertyInfo)targetProperty;
                                pi.SetValue(fe, ie, null);
                            }

                            // Unhook the handler if we are supposed to.
                            if (this.OneTime)
                            {
                                fe.Loaded -= hookup;
                            }
                        };

                        // Wait for the element to load
                        fe.Loaded += deferredFocusHookup;
                    }
                    else
                    {
                        return GetLeafFocusableChild(fe);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Locate the first real focusable child. We keep going down
        /// the visual tree until we hit a leaf node.
        /// </summary>
        /// <param name="fe">Focusable element that is used for finding</param>
        /// <returns>First real focusable child</returns>
        private static IInputElement GetLeafFocusableChild(IInputElement fe)
        {
            IInputElement ie = GetFirstFocusableChild(fe), final = ie;

            while (final != null)
            {
                ie = final;
                final = GetFirstFocusableChild(final);
            }

            return ie;
        }

        /// <summary>
        /// This searches the Visual Tree looking for a valid child which can have focus.
        /// </summary>
        /// <param name="fe">Focusable element that is used for finding</param>
        /// <returns>First real focusable child</returns>
        private static IInputElement GetFirstFocusableChild(IInputElement fe)
        {
            var dpo = fe as DependencyObject;
            return dpo == null ? null : (from vc in EnumerateVisualTree(dpo, c => !FocusManager.GetIsFocusScope(c))
                                         let iic = vc as IInputElement
                                         where iic != null && iic.Focusable && iic.IsEnabled &&
                                         (!(iic is FrameworkElement) || ((FrameworkElement)iic).IsVisible)
                                         select iic).FirstOrDefault();
        }

        /// <summary>
        /// A simple iterator method to expose the visual tree to LINQ
        /// </summary>
        /// <typeparam name="T">Type of the objects to use in iterator</typeparam>
        /// <param name="start">Start element</param>
        /// <param name="eval">Predicate to evaluate</param>
        /// <returns>Iterator object</returns>
        private static IEnumerable<T> EnumerateVisualTree<T>(T start, Predicate<T> eval) 
            where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(start); i++)
            {
                var child = VisualTreeHelper.GetChild(start, i) as T;
                if (child != null && (eval != null && eval(child)))
                {
                    yield return child;
                    foreach (var childOfChild in EnumerateVisualTree(child, eval))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
