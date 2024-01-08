/* PersistentSlider.cs - C's Utils
 * 
 * Allows a slider's value to persist between sessions
 * 
 * 
 * Creation Date: 01/01/2024
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [01/01/2024] - Initial implementation (C137)
 *      [03/01/2024] - Added data sectioning support (C137)
 *                   - Improved event subscription handling (C137)
 *      
 *      [05/01/2024] - Added support for type casting fix (C137)
 *                   - Added missing namespace (C137)
 *      
 *      [06/01/2024] - Slider now uses the new peristent property system (C137)
 */

using UnityEngine;
using UnityEngine.UI;

namespace CsUtils.Systems.DataSaving
{
    public class PersistentSlider : MonoBehaviour
    {
        /// <summary>
        /// The slider whose value should persist
        /// </summary>
        public Slider slider;

        /// <summary>
        /// The id to store the data with
        /// </summary>
        public string id;

        /// <summary>
        /// The id of the section in which to store this persistent data
        /// </summary>
        public string sectionID = "default";

        /// <summary>
        /// The default value to initialize the slider with. Set to -1 to use the current value of the slider
        /// </summary>
        [Tooltip("The default value to initialize the slider with. Set to -1 to use the current value of the slider")]
        public float defaultValue = -1;

        /// <summary>
        /// Stores the slider's value persistently
        /// </summary>
        PersistentProperty<float> persistentValue;

        private void Start()
        {
            if (slider == null)
            {
                CsSettings.Logger.LogDirect("No slider has been assigned. Disabling script", Logging.LogSeverity.Warning, gameObject);
                enabled = false;
                return;
            }

            InitializeSlider();
        }

        /// <summary>
        /// Initializes the slider with its proper values
        /// </summary>
        void InitializeSlider()
        {
            defaultValue = defaultValue == -1 ? slider.value : defaultValue;
            persistentValue = new(id, defaultValue, sectionID);

            slider.onValueChanged.AddListener(SliderValueChanged);
        }

        /// <summary>
        /// Called when the value of the slider changes
        /// </summary>
        /// <param name="value">The new value of the slider</param>
        void SliderValueChanged(float value)
        {
            persistentValue.Value = value;
        }

        /// <summary>
        /// Sets the value of the slider to that of the recorded one
        /// </summary>
        private void LateUpdate()
        {
            slider.value = persistentValue.Value;
        }

        /// <summary>
        /// Sets the proper values
        /// </summary>
        private void Reset()
        {
            TryGetComponent(out slider);

            Transform parent = slider.transform.parent;

            while (parent != null)
            {
                id = $"{parent.name}.{id}";
                parent = parent.parent;
            }

            id = (id + slider.transform.name).Replace(' ', '_').ToLower();
        }
    }
}