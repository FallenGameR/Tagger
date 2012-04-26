//-----------------------------------------------------------------------
// <copyright file="ColorSelectionControl.xaml.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Tagger.Controls
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;
    using System.Windows.Threading;

    using Utils.Extensions;

    /// <summary>
    /// Interaction logic for ColorSelectionControl.xaml
    /// </summary>
    public partial class ColorSelectionControl : UserControl
    {
        /// <summary>
        /// Color that is bound to the control
        /// </summary>
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            "Color", typeof(Color), typeof(ColorSelectionControl), new PropertyMetadata(Colors.Black));

        /// <summary>
        /// Color names resource
        /// </summary>
        /// <remarks>
        /// Not moved to XAML since in C# it's way more readable to call Select from LINQ
        /// </remarks>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate",
            Justification = "Reviewed. Suppression is OK here.")]
        public static string[] ColorNames = typeof(Colors).GetProperties().Select(pi => pi.Name).ToArray();

        /// <summary>
        /// Flags that tracks if control was fully initialized
        /// </summary>
        private bool isFullyInitialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorSelectionControl"/> class.
        /// </summary>
        public ColorSelectionControl()
        {
            this.InitializeComponent();

            this.isFullyInitialized = false;
            this.IsVisibleChanged += this.ColorSelectionControl_IsVisibleChanged;
        }

        /// <summary>
        /// Gets or sets color that is bound to the control
        /// </summary>
        public Color Color
        {
            get { return (Color)this.GetValue(ColorProperty); }
            set { this.SetValue(ColorProperty, value); }
        }

        /// <summary>
        /// Pick a random color
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Color = ColorRandom.Next();
        }

        /// <summary>
        /// Change color picker to advanced mode on control load
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        /// <remarks>
        /// Control is needed to be loaded to use traverse its visual tree.
        /// Control name is taken from ExtendedWPFToolkit sources of ColorPicker control.
        /// </remarks>
        private void ColorSelectionControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.isFullyInitialized)
            {
                return;
            }

            if (this.Visibility == Visibility.Visible)
            {
                this.isFullyInitialized = true;
            }

            var toggleColorModeButtons = (Action)delegate
            {
                Func<DependencyObject, string, bool> name =
                    (d, controlname) => ((string)d.GetValue(NameProperty)) == controlname;

                var toggleButtons = 
                    from visual in this.colorPicker.GetVisualTreeChildren()
                    where name(visual, "PART_ColorPickerPalettePopup")
                    from logical in visual.GetLogicalTreeChildren().OfType<DependencyObject>()
                    where name(logical, "_colorMode")
                    select logical as ToggleButton;

                toggleButtons.Action(b => b.IsChecked = true);
            };

            this.Dispatcher.BeginInvoke(toggleColorModeButtons, DispatcherPriority.Loaded);
        }
    }
}