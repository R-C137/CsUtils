/* ModalWindow.cs - C's Utils
 * 
 * Handles the behaviour for a modal window
 * 
 * 
 * Creation Date: 16/04/2024
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [16/04/2024] - Initial implementation (C137)
 *                   - Added missing namespace (C137)
 *      
 */

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CsUtils
{

    public class ModalWindow : MonoBehaviour
    {
        [Serializable]
        public struct ModalButton
        {
            /// <summary>
            /// Reference to the actual button
            /// </summary>
            public Button button;

            /// <summary>
            /// Reference to the text display of the button
            /// </summary>
            public TextMeshProUGUI textDisplay;
        }

        /// <summary>
        /// The text component used to display the modal question
        /// </summary>
        public TextMeshProUGUI modalQuestionDisplay;

        /// <summary>
        /// Button pressed to confirm the modal question
        /// </summary>
        public ModalButton confirmButton;

        /// <summary>
        /// Button pressed to deny the modal question
        /// </summary>
        public ModalButton denyButton;

        /// <summary>
        /// Sets up the modal window
        /// </summary>
        /// <param name="modalQuestion">The question to diplay on the modal window</param>
        /// <param name="confirm">The callback when the confirm button is pressed</param>
        /// <param name="deny">The callback when the deny button is pressed</param>
        /// <param name="destroyOnComplete">Whether the destroy the modal window when a button is pressed</param>
        /// <param name="confirmButtonText">The text to show on the confirm button</param>
        /// <param name="denyButtonText">The text to show on the deny button</param>
        public void SetupModal(string modalQuestion, Action confirm, Action deny, bool destroyOnComplete = true, string confirmButtonText = "Yes", string denyButtonText = "No")
        {
            modalQuestionDisplay.text = modalQuestion;

            if (destroyOnComplete)
            {
                confirm += () => Destroy(gameObject);
                deny += () => Destroy(gameObject);
            }

            confirmButton.textDisplay.text = confirmButtonText;
            denyButton.textDisplay.text = denyButtonText;

            ResetButtons();
            confirmButton.button.onClick.AddListener(() => confirm?.Invoke());
            denyButton.button.onClick.AddListener(() => deny?.Invoke());
        }

        public void ResetButtons()
        {
            confirmButton.button.onClick.RemoveAllListeners();
            denyButton.button.onClick.RemoveAllListeners();
        }
    }
}