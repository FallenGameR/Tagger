using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils.Extensions
{
    public static class Strings
    {
        /// <summary>
        /// Simplyfied syntax for string format
        /// </summary>
        /// <param name="source">Format string</param>
        /// <param name="action">Arguments to be formated</param>
        /// <returns>Formated arguments</returns>
        public static string Format(this string format, params object[] args)
        {
            return string.Format(format, args);
        }
    }
}
