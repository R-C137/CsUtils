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
 *                   
 *      [30/11/2023] - Fixed a major bug in 'GetIndexesOf<T>(...)' which caused the function to not always returned the queried items (C137)
 *                   - Renamed 'GetIndexesOf<T>(...)' parameter 'T[] search' into 'T[] query' (C137)
 *                   
 *      [05/12/2023] - Added an extension to check if a method should be shown in the stack trace [This extension won't have it's own script as its a few methods] (C137)
 */
using CsUtils.Systems.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

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
        /// <param name="query">Array of the elements to search the index of</param>
        /// <returns></returns>
        public static int[] GetIndexesOf<T>(T[] array, T[] query)
        {
            if (!query.Any())
                return null;

            //Use a custom array to allow the indexed arrays to be different and not indexed again
            IndexFinder<T>[] finder = new IndexFinder<T>[array.Length];

            //Setup the index finder with the proper values
            for (int i = 0; i < array.Length; i++)
            {
                finder[i].element = array[i];
            }

            int[] result = new int[query.Length];

            for (int i = 0; i < query.Length; i++)
            {
                result[i] = Array.IndexOf(finder, new() { element = query[i] });

                finder[result[i]] = new() { element = finder[result[i]].element, indexed = true};
            }

            return result;
        }

        public static string BreakAndIndent(string input, int indent, int maxLength)
        {
            StringBuilder result = new();
            string indentation = new(' ', indent);

            while (input.Length > maxLength)
            {
                string line = input[..maxLength];
                int lastSpace = line.LastIndexOf(' ');

                if (lastSpace > 0)
                {
                    line = line[..lastSpace];
                }

                result.AppendLine(indentation + line);
                input = input[line.Length..].TrimStart();
            }

            result.AppendLine(indentation + input);

            return result.ToString();
        }
    }
}

namespace CsUtils.Extensions
{
    public static class MethodBaseExtensions
    {
        public static bool ShouldHideFromStackTrace(this MethodBase method)
        {
            return method.IsDefined(typeof(HideFromStackTraceAttribute), true);
        }
    }

    public static class ExceptionExtensions
    {
        public static string GetStackTraceWithoutHiddenMethods(this Exception e)
        {
            return string.Concat(
                new StackTrace(e, true)
                    .GetFrames()
                    .Where(frame => !frame.GetMethod().ShouldHideFromStackTrace())
                    .Select(frame => new StackTrace(frame).ToString())
                    .ToArray());  // ^^^^^^^^^^^^^^^     ^
        }                         // required because you want the usual stack trace
    }
}
