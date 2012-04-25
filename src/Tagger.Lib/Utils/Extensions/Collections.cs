//-----------------------------------------------------------------------
// <copyright file="Collections.cs" company="none">
//  Distributed under the 3-clause BSD license
//  Copyright (c) Alexander Kostikov
//  All rights reserved
// </copyright>
//-----------------------------------------------------------------------

namespace Utils.Extensions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Extension class for collections
    /// </summary>
    public static class Collections
    {
        /// <summary>
        /// Execute an action for each element in a collection
        /// </summary>
        /// <typeparam name="T">Type of the collection elements</typeparam>
        /// <param name="source">Collection to be enumerated</param>
        /// <param name="action">Action to be executed</param>
        /// <returns>Original collection</returns>
        public static IEnumerable<T> Action<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source)
            {
                action(element);
            }

            return source;
        }
    }
}
