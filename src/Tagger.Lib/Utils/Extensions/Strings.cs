//-----------------------------------------------------------------------
// <copyright file="Strings.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Utils.Extensions
{
    /// <summary>
    /// Extension class for strings
    /// </summary>
    public static class Strings
    {
        /// <summary>
        /// Simplyfied syntax for string format
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="args">Arguments to be formated</param>
        /// <returns>Formated arguments</returns>
        public static string Format(this string format, params object[] args)
        {
            return string.Format(format, args);
        }
    }
}
