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
        /// Whether the value of the slider should be updated automatically
        /// </summary>
        public bool autoUpdate = true;

        private void Start()
        {
            if (slider == null)
            {
                CsSettings.Logger.LogDirect("No reference to a slider has been set, disabling the persistency", CsUtils.Systems.Logging.LogSeverity.Warning, gameObject);
                enabled = false;
                return;
            }

            defaultValue = defaultValue == -1 ? slider.value : defaultValue;

            UpdateValue(id, GameData.Get(id, sectionID, defaultValue));

            slider.onValueChanged.AddListener(SliderValueChanged);
            GameData.persistenDataSections[sectionID].onDataUpdated += UpdateValue;
        }

        /// <summary>
        /// Called when the value of the slider has been modified
        /// </summary>
        /// <param name="value">The new value of the slider</param>
        public void SliderValueChanged(float value)
        {
            if (autoUpdate)
                GameData.Set(id, value, sectionID);
        }

        /// <summary>
        /// Updates the value of the slider to the stored value
        /// </summary>
        public void UpdateValue(string id, object value)
        {
            if (!autoUpdate)
                return;

            if (this.id == id)
            {
                GameData.FixTypeCasting(value, out float result);
                slider.value = result;
            }
        }

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

        private void OnDisable()
        {
            GameData.persistenDataSections[sectionID].onDataUpdated -= UpdateValue;
        }
    }
}