/* PersistentToggle.cs - C's Utils
 * 
 * Allows a toggle's value to be persistent across sessions
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
 *                   - Better default id (C137)
 */

using UnityEngine;
using UnityEngine.UI;

namespace CsUtils.Systems.DataSaving
{
    public class PersistentToggle : MonoBehaviour
    {
        /// <summary>
        /// The toggle whose value should persist
        /// </summary>
        public Toggle toggle;

        /// <summary>
        /// The id to store the data with
        /// </summary>
        public string id;

        /// <summary>
        /// The id of the section in which to store this persistent data
        /// </summary>
        public string sectionID = "default";

        /// <summary>
        /// The default value to initialize the toggle with.
        /// </summary>
        public bool defaultValue;

        /// <summary>
        /// Stores the toggle's value persistently
        /// </summary>
        PersistentProperty<bool> persistentValue;

        private void Start()
        {
            if (toggle == null)
            {
                CsSettings.Logger.LogDirect("No toggle has been assigned. Disabling script", Logging.LogSeverity.Warning, gameObject);
                enabled = false;
                return;
            }

            InitializeToggle();
        }

        /// <summary>
        /// Initializes the toggle with its proper values
        /// </summary>
        void InitializeToggle()
        {
            persistentValue = new(id, defaultValue, sectionID);

            toggle.onValueChanged.AddListener(SliderValueChanged);
        }

        /// <summary>
        /// Called when the value of the toggle changes
        /// </summary>
        /// <param name="value">The new toggle of the slider</param>
        void SliderValueChanged(bool value)
        {
            persistentValue.Value = value;
            Debug.Log("value changed");
        }

        /// <summary>
        /// Sets the value of the slider to that of the recorded one
        /// </summary>
        private void LateUpdate()
        {
            toggle.isOn = persistentValue.Value;
        }

        /// <summary>
        /// Sets the proper values 
        /// </summary>
        private void Reset()
        {
            if(TryGetComponent(out toggle))
            {
                defaultValue = toggle.isOn;
            }

            foreach (Transform parent in StaticUtils.GetParents(toggle.transform))
            {
                id = $"{parent.name}.{id}";
            }

            id = (id + toggle.name).Replace(' ', '_').ToLower();
        }

        /// <summary>
        /// Adjust the value of the toggle properly
        /// </summary>
        private void OnValidate()
        {
            if(toggle != null)
                toggle.isOn = defaultValue;
        }
    }
}