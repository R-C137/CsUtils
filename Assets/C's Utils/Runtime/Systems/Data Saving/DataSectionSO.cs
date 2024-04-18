/* DataSectionSO.cs - C's Utils
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
 */
using System;
using System.IO;
using UnityEngine;
namespace CsUtils.Systems.DataSaving
{
    [CreateAssetMenu(fileName = "Data Section", menuName = "C's Utils/Data Saving/Data Section", order = 1)]
    public class DataSectionSO : ScriptableObject
    {
#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// The path at which the data for this section will be stored
        /// </summary>
        public string dataPath
        {
            get
            {
                if (dataPathUpdated)
                    return _dataPath;

                return UpdateDataPath();
            }
            set
            {
                
            }
        }
#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// The identifier with which this section will be accessible
        /// </summary>
        public string sectionID;

        /// <summary>
        /// Whether the data path has been updated with the environment variables and custom definitions
        /// </summary>
        public bool dataPathUpdated = false;

        /// <summary>
        /// The actual holder for the path at which the data for this section will be stored
        /// </summary>
        [SerializeField, InspectorName("Data Path")]
        string _dataPath;

        string UpdateDataPath()
        {
            dataPathUpdated = true;
            _dataPath = Environment.ExpandEnvironmentVariables(CsSettings.singleton.ReplaceCustomDefinitions(_dataPath));

            return _dataPath;
        }

        private void Reset()
        {
            _dataPath = Path.Combine(
                CsSettings.hasInstance ? Path.GetDirectoryName(CsSettings.singleton.dataSavingPath) : Path.Combine("%appddata%",
                "%unity.companyName%", "%unity.productName%", "Data"), Guid.NewGuid().ToString() + ".bin");
        }
    }
}