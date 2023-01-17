using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TDC.Core.Extension
{
    public static class ListEx
    {
        /// <summary>
        /// Random element between 0 & List length.
        /// </summary>
        /// <typeparam name="T">Type of List</typeparam>
        /// <param name="list">List collection</param>
        /// <param name="rnd"></param>
        /// <returns>Random element.</returns>
        public static T Random<T>(this List<T> list, System.Random rnd) => 
            list.Count <= 0 ? default : list[rnd.Next(0, list.Count)];
        public static T Random<T>(this ReadOnlyCollection<T> collection, System.Random rnd) => 
            collection.Count <= 0 ? default : collection[rnd.Next(0, collection.Count)];

        // public static T Random<T>(this List<T> list) => Random(list, new Random());

        public static bool InBounds<T>(this List<T> list, int index)
        {
            return !(index > list.Count - 1 && index < 0);
        }
    }
}