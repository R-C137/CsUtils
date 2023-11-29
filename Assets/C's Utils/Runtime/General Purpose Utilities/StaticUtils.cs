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
 */
using System;
using System.Linq;

namespace CsUtils
{
    public static class StaticUtils
    {
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

            int[] result = new int[search.Count()];

            for (int i = 0; i < search.Count(); i++)
            {
                result[i] = Array.IndexOf(array, search[i]);
            }

            return result;
        }
    }
}
