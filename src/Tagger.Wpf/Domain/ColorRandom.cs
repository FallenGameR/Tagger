//-----------------------------------------------------------------------
// <copyright file="ColorRandom.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Tagger
{
    using System;
    using System.Linq;
    using System.Windows.Media;

    /// <summary>
    /// Returns random visible color 
    /// </summary>
    public static class ColorRandom
    {
        /// <summary>
        /// All named visible colors
        /// </summary>
        private static readonly Color[] VisibleColors = (
            from property in typeof(Colors).GetProperties()
            where property.Name != "Transparent"
            select (Color)property.GetValue(null, null)).ToArray();

        /// <summary>
        /// Get next random visible color 
        /// </summary>
        /// <returns>
        /// Next random named color
        /// </returns>
        public static Color Next()
        {
            var random = new Random();
            var toSkip = random.Next(0, VisibleColors.Count());
            return VisibleColors.Skip(toSkip).First();
        }
    }
}