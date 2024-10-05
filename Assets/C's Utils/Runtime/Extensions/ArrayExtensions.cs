/* ArrayExtensions.cs - C's Utils
 * 
 * Contains various QoL extensions to Enumerables
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
 *      [03/12/2023] - Enumerable formatting Utility (C137)
 *      [25/12/2023] - Added an extension to get a random element from an Enumerable (C137)
 */

using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Gets a random element from an IEnumerable<br></br>
        /// Will iterate twice if no weights are passed
        /// </summary>
        /// <typeparam name="T">The type of the enumerable</typeparam>
        /// <param name="enumerable">The IEnumerable on which the operation shall take place</param>
        /// <param name="weights">The weights affecting the randomness of the elements</param>
        /// <returns>A random element from the IEnumerable selected based on the weight</returns>
        public static T RandomElement<T>(this IEnumerable<T> enumerable, WeightedNumber[] weights = null)
        {
            weights ??= GenerateWeights();

            return enumerable.ElementAt(StaticUtils.WeightedRandom(weights));

            WeightedNumber[] GenerateWeights()
            {
                WeightedNumber[] weights = new WeightedNumber[enumerable.Count()];

                int count = enumerable.Count();
                for (int i = 0; i < count; i++)
                {
                    weights[i] = new WeightedNumber()
                    {
                        number = i,
                        probability = 1
                    };
                }

                return weights;
            }
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
