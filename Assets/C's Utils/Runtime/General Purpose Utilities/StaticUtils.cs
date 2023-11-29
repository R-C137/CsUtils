/* StaticUtils.cs - C's Utils
 * 
 * A class containing various QoL utilities 
 * 
 * 
 * Creation Date: 29/11/2023
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [29/11/2023] - Initial implementation (C137)
 *                   - Fixed a major bug in 'GetIndexesOf<T>(...)' which caused the first index to always be returned (C137)
 */
using System;
using System.Linq;

namespace CsUtils
{

    public static class StaticUtils
    {
        public struct IndexFinder<T>
        {
            public T element;

            public bool indexed;
        }

        /// <summary>
        /// Returns the indexes of multiple elements from an array
        /// </summary>
        /// <typeparam name="T">The type of the object and array to query</typeparam>
        /// <param name="array">The array in which the query shall be ran</param>
        /// <param name="search">Array of the elements to search the index of</param>
        /// <returns></returns>
        public static int[] GetIndexesOf<T>(T[] array, T[] search)
        {
            if (!search.Any())
                return null;

            //Use a custom array to allow the indexed arrays to be different and not indexed again
            IndexFinder<T>[] finder = new IndexFinder<T>[array.Length];

            //Setup the index finder with the proper values
            for (int i = 0; i < array.Length; i++)
            {
                finder[i].element = array[i];
            }

            int[] result = new int[finder.Length];

            for (int i = 0; i < finder.Length; i++)
            {
                result[i] = Array.IndexOf(finder, new() { element = array[i] });

                finder[result[i]] = new() { element = finder[result[i]].element, indexed = true};
            }

            return result;
        }
    }
}
