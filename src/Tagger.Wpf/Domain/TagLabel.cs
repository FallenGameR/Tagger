//-----------------------------------------------------------------------
// <copyright file="TagLabel.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Tagger
{
    using System;
    using System.Windows.Media;
    using Utils.Diagnostics;

    /// <summary>
    /// Label taht is used to group similiar tags together
    /// </summary>
    /// <remarks>
    /// Only tag text and tag color are significant
    /// </remarks>
    public class TagLabel : IEquatable<TagLabel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TagLabel"/> class. 
        /// </summary>
        /// <param name="viewModel">
        /// ViewModel that defines all tag visible properties
        /// </param>
        public TagLabel(TagViewModel viewModel)
        {
            Check.Require(viewModel != null, "View model must not be null");

            this.Text = viewModel.Text;
            this.Color = viewModel.Color;
        }

        /// <summary>
        /// Gets text of the tag
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Gets color of the tag (the background color, not font color)
        /// </summary>
        public Color Color { get; private set; }

        /// <summary>
        /// Gets hash code for the object
        /// </summary>
        /// <returns>Numeric hash code that intended to be used in hash tables</returns>
        public override int GetHashCode()
        {
            return this.Text.GetHashCode() ^ this.Color.GetHashCode();
        }

        /// <summary>
        /// Checks object equality
        /// </summary>
        /// <param name="obj">Other object to compare to</param>
        /// <returns>true if object are semantically equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            var other = obj as TagLabel;
            return other != null && this.Equals(other);
        }

        /// <summary>
        /// Checks tag label equality
        /// </summary>
        /// <param name="other">Other tag label to compare to</param>
        /// <returns>true if tag labels are semantically equal, false otherwise</returns>
        public bool Equals(TagLabel other)
        {
            return this.Text == other.Text
                && this.Color == other.Color;
        }
    }
}