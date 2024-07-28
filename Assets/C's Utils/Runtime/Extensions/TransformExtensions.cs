/* TransformExtensions.cs - C's Utils
 * 
 * Provides various QoL extensions for manipulating the 'Transform' class
 * 
 * 
 * Creation Date: 28/07/2024
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [28/07/2024] - Initial implementation (C137)
 *      
 */
using System.Collections.Generic;
using UnityEngine;

namespace CsUtils.Extensions
{
    public static class TransformExtensions
    {

        /// <summary>
        /// Returns all the parents of a transform
        /// </summary>
        /// <param name="child">The child whom to query for its parents</param>
        /// <returns>An list of the parents of the child from closet to furthest</returns>
        public static List<Transform> GetParents(this Transform child)
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
        /// Destroys all the children in a parent
        /// </summary>
        /// <param name="transform">The parent whose children are to be destroyed</param>
        public static void DestroyChildren(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                Object.Destroy(child.gameObject);
            }
        }

    }
}
