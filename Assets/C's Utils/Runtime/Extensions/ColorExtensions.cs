/* ColorExtensions.cs - C's Utils
 * 
 * Provides various QoL extensions for manipulating the 'Color' class
 * 
 * 
 * Creation Date: 28/11/2023
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [28/11/2023] - Initial implementation (C137)
 */
using UnityEngine;
namespace CsUtils.Extensions
{
    public static class ColorExtensions
    {
        #region Color

        #region Floating Point Value Changes
        /// <summary>
        /// Changes the red value of the color
        /// </summary>
        /// <param name="red">The red value to be set</param>
        /// <returns>The color with the red value set to the predefined one</returns>
        public static Color WithRed(this Color color, float red) => new(red, color.g, color.b, color.a);

        /// <summary>
        /// Changes the green value of the color
        /// </summary>
        /// <param name="green">The green value to be set</param>
        /// <returns>The color with the green value set to the predefined one</returns>
        public static Color WithGreen(this Color color, float green) => new(color.r, green, color.b, color.a);

        /// <summary>
        /// Changes the blue value of the color
        /// </summary>
        /// <param name="blue">The blue value to be set</param>
        /// <returns>The color with the blue value set to the predefined one</returns>
        public static Color WithBlue(this Color color, float blue) => new(color.r, color.g, blue, color.a);

        /// <summary>
        /// Changes the alpha of the color
        /// </summary>
        /// <param name="alpha">The alpha to be set</param>
        /// <returns>The color with the alpha set to the predefined one</returns>
        public static Color WithAlpha(this Color color, float alpha) => new(color.r, color.g, color.b, alpha);
        #endregion

        #region Integer Value Changes
        /// <summary>
        /// Changes the red value of the color
        /// </summary>
        /// <param name="red">The red value to be set</param>
        /// <returns>The color with the red value set to the predefined one</returns>
        public static Color WithRed(this Color color, int red) => new(red / 255f, color.g, color.b, color.a);

        /// <summary>
        /// Changes the green value of the color
        /// </summary>
        /// <param name="green">The green value to be set</param>
        /// <returns>The color with the green value set to the predefined one</returns>
        public static Color WithGreen(this Color color, int green) => new(color.r, green / 255f, color.b, color.a);

        /// <summary>
        /// Changes the blue value of the color
        /// </summary>
        /// <param name="blue">The blue value to be set</param>
        /// <returns>The color with the blue value set to the predefined one</returns>
        public static Color WithBlue(this Color color, int blue) => new(color.r, color.g, blue / 255f, color.a);

        /// <summary>
        /// Changes the alpha of the color
        /// </summary>
        /// <param name="alpha">The alpha to be set</param>
        /// <returns>The color with the alpha set to the predefined one</returns>
        public static Color WithAlpha(this Color color, int alpha) => new(color.r, color.g, color.b, alpha / 255f);
        #endregion

        #endregion

        #region Color32
        /// <summary>
        /// Changes the red value of the color
        /// </summary>
        /// <param name="red">The red value to be set</param>
        /// <returns>The color with the red value set to the predefined one</returns>
        public static Color32 WithRed(this Color32 color, byte red) => new(red, color.g, color.b, color.a);

        /// <summary>
        /// Changes the green value of the color
        /// </summary>
        /// <param name="green">The green value to be set</param>
        /// <returns>The color with the green value set to the predefined one</returns>
        public static Color32 WithGreen(this Color32 color, byte green) => new(color.r, green, color.b, color.a);

        /// <summary>
        /// Changes the blue value of the color
        /// </summary>
        /// <param name="blue">The blue value to be set</param>
        /// <returns>The color with the blue value set to the predefined one</returns>
        public static Color32 WithBlue(this Color32 color, byte blue) => new(color.r, color.g, blue, color.a);

        /// <summary>
        /// Changes the alpha of the color
        /// </summary>
        /// <param name="alpha">The alpha to be set</param>
        /// <returns>The color with the alpha set to the predefined one</returns>
        public static Color32 WithAlpha(this Color32 color, byte alpha) => new(color.r, color.g, color.b, alpha);
        #endregion

        //Whilst an implicit cast exist to and from 'Color32', an extension doing so is provided for convenience

        /// <summary>
        /// Converts 'Color' to its 'Color32' counterpart
        /// </summary>
        /// <returns>'Color' implicitly casted to 'Color32'</returns>
        public static Color32 ToColor32(this Color color) => color;

        /// <summary>
        /// Converts 'Color32' to its 'Color' counterpart
        /// </summary>
        /// <returns>'Color32' implicitly casted to 'Color'</returns>
        public static Color ToColor(this Color32 color) => color;
    }
}
