/* TextChanger.cs - C's Utils
 * 
 * An animation utility which changes the value of a text over time
 * 
 * 
 * Creation Date: 06/12/2023
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [06/12/2023] - Initial implementation (C137)
 *      [09/12/2023] - Removed unnecessary using statements (C137)
 */
using System;
using System.Collections;
using CsUtils.Modules.LeanTween;
using TMPro;
using UnityEngine;

namespace CsUtils.UI
{
    [Serializable]
    public struct TextChange
    {
        /// <summary>
        /// The text to show
        /// </summary>
        public string text;

        /// <summary>
        /// How long should the text be shown
        /// </summary>
        public float displayTime;
    }

    public class TextChanger : MonoBehaviour
    {
        /// <summary>
        /// The changes to be made
        /// </summary>
        public TextChange[] textChanges;

        /// <summary>
        /// Whether to loop the animation
        /// </summary>
        public bool loop = true;

        /// <summary>
        /// The delay for starting the animation
        /// </summary>
        public float delay;

        /// <summary>
        /// The text on which the changes will take place
        /// </summary>
        public TMP_Text animatedText;

        /// <summary>
        /// The current index displayed
        /// </summary>
        int currentIndex;

        /// <summary>
        /// Whether the animation has already started
        /// </summary>
        bool animationStarted = false;

        /// <summary>
        /// The coroutine handling the animation
        /// </summary>
        Coroutine animationCoroutine;

        public void Start()
        {
            LeanTween.delayedCall(delay, () => animationCoroutine = StartCoroutine(TextChangeAnimation()));
            animationStarted = true;
        }

        public IEnumerator TextChangeAnimation()
        {
            while (true)
            {
                animatedText.text = textChanges[currentIndex].text;

                yield return new WaitForSeconds(textChanges[currentIndex].displayTime);

                if ((currentIndex += 1) >= textChanges.Length) 
                {
                    if (loop)
                        currentIndex = 0;
                    else
                        break;
                }

            }
        }

        /// <summary>
        /// Automatically set the animated text
        /// </summary>
        public void Reset()
        {
            TryGetComponent(out animatedText);
        }

        /// <summary>
        /// Disable the coroutine when the game object becomes inactive
        /// </summary>
        private void OnDisable()
        {
            StopCoroutine(animationCoroutine);
        }

        /// <summary>
        /// Restart the coroutine when the game object is re-enabled
        /// </summary>
        private void OnEnable()
        {
            if(animationStarted)
                animationCoroutine = StartCoroutine(TextChangeAnimation());
        }
    }
}
