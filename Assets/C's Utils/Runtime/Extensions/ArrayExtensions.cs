/* ArrayExtensions.cs - C's Utils
 * 
 * Contains various QoL extensions to arrays
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
 *      [03/12/2023] - Enumerable Formatting Utility (C137)
 */

using System;
using System.Collections.Generic;

namespace CsUtils.Extensions
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Replaces an element within an array
        /// </summary>
        /// <typeparam name="T">The type of the array</typeparam>
        /// <param name="array">The array on which the operation shall be done</param>
        /// <param name="index">The index at which the element sall be replaced</param>
        /// <param name="element">The element to replace the index with</param>
        /// <returns>The element that was at previously at the index</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static T Replace<T>(this T[] array, int index, T element)
        {
            if(index < 0 || index >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            T removedElement = array[index];

            array[index] = element;

            return removedElement;

        }

        /// <summary>
        /// Replaces an element within a list
        /// </summary>
        /// <typeparam name="T">The type of the list</typeparam>
        /// <param name="list">The list on which the operation shall be done</param>
        /// <param name="index">The index at which the element sall be replaced</param>
        /// <param name="element">The element to replace the index with</param>
        /// <returns>The element that was at previously at the index</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static T Replace<T>(this List<T> list, int index, T element)
        {
            if (index < 0 || index >= list.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            T removedElement = list[index];

            list[index] = element;

            return removedElement;

        }

        /// <summary>
        /// Formats an en into a better string format by showing its contents
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable">The enumerable to format</param>
        /// <returns></returns>
        public static string Format<T>(this IEnumerable<T> enumerable)
        {
            return "{ " + string.Join(',', enumerable) + " }";
        }
    }
}
