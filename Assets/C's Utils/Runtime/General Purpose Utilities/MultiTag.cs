/* MultiTag.cs - C's Utils
 * 
 * When added to a GameObject, allows it to have multiple tags
 * 
 * 
 * Creation Date: 29/11/2023
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [30/11/2023] - Initial implementation (C137)
 */
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CsUtils
{
    public class MultiTag : MonoBehaviour
    {
        /// <summary>
        /// Whether to also use the default unity tag system on top of the additional ones
        /// </summary>
        public bool useDefaultUnityTag = true;

        /// <summary>
        /// The getter for the tags
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        public string[] tags
        { 
            get
            {
                if (_tags == null || !_tags.Any())
                    return useDefaultUnityTag ? new string[] { gameObject.tag } : new string[] { "Untagged" };
                else
                    return useDefaultUnityTag ? _tags.Append(gameObject.tag).ToArray() : _tags.ToArray();
            } 
            set 
            { 
                _tags = value; 
            } 
        }
#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// The additional tags setup by the user. Although set to public, scripts shouldn't access this
        /// </summary>
        [InspectorName("Tags")]
        public string[] _tags;

    }

}
namespace CsUtils.Extensions
{
    public static class MultiTagExtension
    {
        /// <summary>
        /// Checks if that GameObject has the requested tag.
        /// </summary>
        /// <param name="obj">The GameObject concerned</param>
        /// <param name="tag">The tag to check for</param>
        /// <returns></returns>
        public static bool HasTag(this GameObject obj, string tag)
        {
            if (obj == null) 
                return false;

            if (obj.TryGetComponent(out MultiTag multiTag))
                return multiTag.tags.Contains(tag);

            return obj.CompareTag(tag);
        }

        /// <summary>
        /// Adds a tag to the GameObject
        /// </summary>
        /// <param name="obj">The GameObject concerned</param>
        /// <param name="tag">The tag to add</param>
        /// <param name="useMultiTag">Whether to use the multi tag system (if applicable)</param>
        public static void AddTag(this GameObject obj, string tag, bool useMultiTag = false)
        {
            if (obj == null)
                return;

            if (useMultiTag && obj.TryGetComponent(out MultiTag multiTag))
            {
                multiTag.tags = multiTag._tags.Append(tag).ToArray();
                return;
            }

            obj.tag = tag;
        }
    }
}
