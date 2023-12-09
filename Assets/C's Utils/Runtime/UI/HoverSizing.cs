/* HoverSizing.cs - C's Utils
 * 
 * Multiplies the font size by a given amount when the mouse is hovered over the concerned UI element
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
 *      [29/11/2023] - Added proper namespace (C137)
 *      [03/12/2023] - Log system support (C137)
 *      [09/12/2023] - Default value are now properly set (C137)
 *                   - Removed unnecessary using statements (C137)
 */
using CsUtils.Systems.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CsUtils.UI
{
    public class HoverSizing : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// The text on which the hover animation sizing should be done
        /// </summary>
        public TMP_Text animatedText;

        /// <summary>
        /// By how much should the font size be multiplied
        /// </summary>
        public float sizeMultiplier = 1.5f;

        /// <summary>
        /// The original font size of the text before animating
        /// </summary>
        float originalFontSize;

        /// <summary>
        /// Whether auto font sizing was enabled before animating
        /// </summary>
        bool autoSizing;

        void Start()
        {
            if (animatedText.gameObject != gameObject)
            {
                CsSettings.Logger.Log("The referenced 'animatedText' isn't on the same GameObject as the 'HoverSizing' script. Pointer events will not register properly", LogSeverity.Warning, gameObject);
            }
        }

        private void Reset()
        {
            transform.TryGetComponent(out animatedText);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            animatedText.fontSize = originalFontSize;
            animatedText.enableAutoSizing = autoSizing;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            originalFontSize = animatedText.fontSize;
            autoSizing = animatedText.enableAutoSizing;

            animatedText.fontSize *= sizeMultiplier;
            animatedText.enableAutoSizing = false;
        }
    }
}
