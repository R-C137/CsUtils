/* DataSectionSo.cs - C's Utils
 * 
 * A scriptable object used to define the various sections in which data is stored
 * 
 * 
 * Creation Date: 03/01/2024
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [03/01/2024] - Initial implementation (C137)
 *      [05/01/2024] - Added missing namespace (C137)
 *      [17/04/2024] - Added support for environment variables in file paths (C137)
 *      [29/04/2024] - Updated code structure (C137)
 *                   - Data path is now only updated at runtime (C137)
 *
 *      [22/11/2024] - Fixed paths being changed at runtime (C137)
 *                   - Improved variable naming (C137)
 *                   - Updated field names (C137)
 */
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;
namespace CsUtils.Systems.DataSaving
{
    [CreateAssetMenu(fileName = "Data Section", menuName = "C's Utils/Data Saving/Data Section", order = 1)]
    public class DataSectionSo : ScriptableObject
    {

        /// <summary>
        /// The expanded path at which the data for this section will be stored
        /// </summary>
        public string dataPath => GetDataPath();

        /// <summary>
        /// The identifier with which this section will be accessible
        /// </summary>
        public string sectionID;

        /// <summary>
        /// The data path, which has been updated with the environment variables and custom definitions.
        /// </summary>
        [NonSerialized]
        private string updatedDataPath = string.Empty;

        /// <summary>
        /// The raw, unexpanded data path where data will be stored for this section.
        /// </summary>
        [FormerlySerializedAs("_dataPath"),SerializeField, InspectorName("Data Path")]
        string rawDataPath;

        string GetDataPath()
        {
            if (updatedDataPath != string.Empty)
                return updatedDataPath;

            return UpdateDataPath();
        }

        string UpdateDataPath()
        {
            updatedDataPath = Environment.ExpandEnvironmentVariables(Singleton.Get<CsSettings>().ReplaceCustomDefinitions(rawDataPath));

            return updatedDataPath;
        }

        private void Reset()
        {
            rawDataPath = Path.Combine(CsSettings.RawDataSavingFolderPath, Guid.NewGuid() + ".bin");
        }
    }
}