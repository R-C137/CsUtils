/* VectorExtensions.cs - C's Utils
 * 
 * Provides various QoL extensions for manipulating Vectors
 * 
 * 
 * Creation Date: 08/01/2024
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [08/01/2024] - Initial implementation (C137)
 */
using UnityEngine;

namespace CsUtils.Extensions
{
    public static class VectorExtensions
    {
        /// <summary>
        /// Sets the x value of a vector3
        /// </summary>
        /// <param name="vector">The vector whose x value is to change</param>
        /// <param name="x">The new x value of the vector</param>
        /// <returns>The vector3 with its x value modified</returns>
        public static Vector3 WithX(this Vector3 vector, float x)
        {
            return new(x, vector.y, vector.z);
        }

        /// <summary>
        /// Sets the y value of a vector3
        /// </summary>
        /// <param name="vector">The vector whose y value is to change</param>
        /// <param name="y">The new y value of the vector</param>
        /// <returns>The vector3 with its y value modified</returns>
        public static Vector3 WithY(this Vector3 vector, float y)
        {
            return new(vector.x, y, vector.z);
        }

        /// <summary>
        /// Sets the z value of a vector3
        /// </summary>
        /// <param name="vector">The vector whose z value is to change</param>
        /// <param name="z">The new z value of the vector</param>
        /// <returns>The vector3 with its z value modified</returns>
        public static Vector3 WithZ(this Vector3 vector, float z)
        {
            return new(vector.x, vector.y, z);
        }

        /// <summary>
        /// Sets the x value of a vector2
        /// </summary>
        /// <param name="vector">The vector whose x value is to change</param>
        /// <param name="x">The new x value of the vector</param>
        /// <returns>The vector2 with its x value modified</returns>
        public static Vector2 WithX(this Vector2 vector, float x)
        {
            return new(x, vector.y);
        }

        /// <summary>
        /// Sets the y value of a vector2
        /// </summary>
        /// <param name="vector">The vector whose y value is to change</param>
        /// <param name="y">The new y value of the vector</param>
        /// <returns>The vector2 with its y value modified</returns>
        public static Vector2 WithY(this Vector2 vector, float y)
        {
            return new(vector.x, y);
        }

        /// <summary>
        /// Finds the direction from the current vector to the destination vector
        /// </summary>
        /// <param name="vector">The current vector to find the direction from</param>
        /// <param name="destination">The vector to find the direction to</param>
        /// <returns>The normalized direction from the current vector to the destination vector</returns>
        public static Vector3 Direction(this Vector3 vector, Vector3 destination)
        {
            return (destination - vector).normalized;
        }
    }
}