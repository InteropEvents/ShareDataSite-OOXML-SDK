using System;
using System.Collections.Generic;
using System.Text;

namespace ShareDataService
{
    /// <summary>
    /// Local Extensions.
    /// </summary>
    public static class LocalExtensions
    {
        /// <summary>
        /// Local extension StringConcatenate method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">source.</param>
        /// <param name="func">Lambda expression.</param>
        /// <returns>String.</returns>
        public static string StringConcatenate<T>(this IEnumerable<T> source, Func<T, string> func)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T item in source)
            {
                sb.Append(func(item));
            }

            return sb.ToString();
        }
    }
}
