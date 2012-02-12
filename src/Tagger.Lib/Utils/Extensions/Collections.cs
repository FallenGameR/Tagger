using System;
using System.Collections.Generic;

namespace Utils.Extensions
{
    public static class Collections
    {
        /// <summary>
        /// Execute an action for each element in a collection
        /// </summary>
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
