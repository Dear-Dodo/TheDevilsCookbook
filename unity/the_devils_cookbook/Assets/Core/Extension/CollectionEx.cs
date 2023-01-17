using System;
using System.Collections.Generic;

namespace TDC.Core.Extension
{
    public static class CollectionEx
    {
        /// <summary>
        /// Returns the index of the first element where <paramref name="condition"/> is true.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="condition"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int FirstIndexOf<T>(this IEnumerable<T> collection, Func<T, bool> condition)
        {
            var currentIndex = 0;
            foreach (T t in collection)
            {
                if (condition(t)) return currentIndex;
                currentIndex++;
            }

            return -1;
        }
    }
}