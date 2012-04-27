//-----------------------------------------------------------------------
// <copyright file="TagControlWindow.xaml.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Tagger.Wpf.Windows
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;

    /// <summary>
    /// Settings window that sets up how tag looks like
    /// </summary>
    /// <remarks>
    /// Only view-related code here
    /// </remarks>
    public partial class TagControlWindow : Window
    {
        /// <summary>
        /// Color names resource
        /// </summary>
        /// <remarks>
        /// Is not moved to XAML since in C# it's way more readable to call Select from LINQ
        /// </remarks>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate",
            Justification = "Reviewed. Suppression is OK here.")]
        public static string[] ColorNames = typeof(Colors).GetProperties().Select(pi => pi.Name).ToArray();

        /// <summary>
        /// Initializes a new instance of the <see cref="TagControlWindow"/> class. 
        /// </summary>
        public TagControlWindow()
        {
            InitializeComponent();

            // Set focus and select all tag text on settings dialog shown
            this.IsVisibleChanged += (sender, e) =>
            {
                if ((bool)e.NewValue)
                {
                    this.SelectTagText();
                }
            };
        }

        /// <summary>
        /// Workaround for late data binding for the settings dialog shown for the first time
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void TextTxt_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            this.SelectTagText();
        }

        /// <summary>
        /// Focus and select all text of the tag to make it easily editable
        /// </summary>
        private void SelectTagText()
        {
            this.TextTxt.Focus();
            this.TextTxt.SelectAll();
        }
    }
}