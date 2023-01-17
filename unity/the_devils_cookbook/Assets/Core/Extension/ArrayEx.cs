using System;

namespace TDC.Core.Extension
{
    public static class ArrayEx
    {
        /// <summary>
        /// Random element between 0 & array length.
        /// </summary>
        /// <typeparam name="T">Type of array</typeparam>
        /// <param name="array">Array collection</param>
        /// <param name="rnd">Random Generator</param>
        /// <returns>Random element.</returns>
        public static T Random<T>(this T[] array, System.Random rnd) => array.Length <= 0 ? default : array[rnd.Next(0, array.Length)];

        public static T Random<T>(this T[] array) => Random(array, new Random());

        public static bool InRange(this Array arr, params int[] indices)
        {
            if (indices.Length != arr.Rank) throw new ArgumentException($"Index count must match array dimensions.");

            for (var i = 0; i < arr.Rank; i++)
            {
                if (indices[i] < 0 || indices[i] >= arr.GetLength(i)) return false;
            }

            return true;
        }

        public static bool TryGetValue<T>(this Array arr, out T value, params int[] indices)
        {
            if (indices.Length != arr.Rank) throw new ArgumentException($"Index count must match array dimensions.");

            try
            {
                value = (T)arr.GetValue(indices);
                return true;
            }
            catch (IndexOutOfRangeException)
            {
                value = default;
                return false;
            }
        }
    }
}