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
 *      [30/11/2023] - Fixed a major bug in 'GetIndexesOf<T>(...)' which caused the function to not always return the queried items (C137)
 *                   - Renamed 'GetIndexesOf<T>(...)' parameter 'T[] search' into 'T[] query' (C137)
 *                   
 *      [05/12/2023] - Added an extension to check if a method should be shown in the stack trace [This extension won't have it's own script as its only a few methods] (C137)
 *      
 *      [08/12/2023] - Added missing summaries (C137)
 *                   - Added support for weighted randomness (C137)
 *                   - Moved string extensions to its own namespace (C137)
 *                   - Updated accessibility of IndexFinder<T> (C137)
 *     
 *      [09/12/2023] - Added support for convert hexadecimal colors into RGB(A) colors (C137)
 *      
 *      [13/12/2023] - Renamed functions (C137)
 *                   - Added extensions for getting a random point within a collider (C137)
 *      
 *      [08/01/2024] - Added an utility to get all the parents of a transform (C137)
 *      [10/01/2024] - Made class a partial one (C137)
 *      [07/03/2024] - ColorFromHex(...) no longer needs a '#' at the start of the hex string (C137)
 *      [16/04/2024] - Added modal window support (C137)
 *                   - Moved 'WeightedRandom()' and 'ColiderExtensions' to their own class (C137)
 *                   - Renamed 'ModalWindow(..)' to 'CreateModalWindow(...)' (C137)
 *      
 *      [01/05/2024] - Fixed grammatical mistakes in summaries (C137)
 *      
 *  TODO:
 *      Add object pooling functionality to modal window
 */
using CsUtils.Systems.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CsUtils
{

    public static partial class StaticUtils
    {
        #region IndexFinding
        private struct IndexFinder<T>
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
        #endregion

        /// <summary>
        /// Converts a hexadecimal color to an RGB(A) color
        /// </summary>
        /// <param name="hex">The hexadecimal color to convert</param>
        /// <returns></returns>
        public static Color ColorFromHex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex.StartsWith('#') ? hex : '#' + hex, out Color result);

            return result;
        }

        /// <summary>
        /// Returns all the parents of a transform
        /// </summary>
        /// <param name="child">The child whom to query for its parents</param>
        /// <returns>An list of the parents of the child from closet to furthest</returns>
        public static List<Transform> GetParents(Transform child)
        {
            List<Transform> parents = new();
            Transform parent = child.parent;

            while (parent != null)
            {
                parents.Add(parent);
                parent = parent.parent;
            }

            return parents;
        }

        /// <summary>
        /// Creates a new canvas with a modal window
        /// </summary>
        /// <param name="modalQuestion">The question to diplay on the modal window</param>
        /// <param name="confirm">The callback when the confirm button is pressed</param>
        /// <param name="deny">The callback when the deny button is pressed</param>
        /// <param name="confirmButtonText">The text to show on the confirm button</param>
        /// <param name="denyButtonText">The text to show on the deny button</param>
        /// <returns></returns>
        public static ModalWindow CreateModalWindow(string modalQuestion, Action confirm, Action deny, string confirmButtonText = "Yes", string denyButtonText = "No")
        {
            GameObject modalObj = UnityEngine.Object.Instantiate(CsSettings.singleton.modalWindowPrefab);

            ModalWindow modal = modalObj.transform.GetChild(0).GetComponent<ModalWindow>();

            modal.SetupModal(modalQuestion, confirm, deny, true, confirmButtonText, denyButtonText);

            return modal;
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

    public static class StringExtensions
    {
        /// <summary>
        /// Breaks and indents a string
        /// </summary>
        /// <param name="input">The string affected</param>
        /// <param name="indent">How much should each line be indented</param>
        /// <param name="maxLength">The maximum length of a line (excluding indent)</param>
        /// <returns></returns>
        public static string BreakAndIndent(this string input, int indent, int maxLength)
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
