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
 */
using System;
using System.IO;
using UnityEngine;
namespace CsUtils.Systems.DataSaving
{
    [CreateAssetMenu(fileName = "Data Section", menuName = "C's Utils/Data Saving/Data Section", order = 1)]
    public class DataSectionSO : ScriptableObject
    {
        /// <summary>
        /// The path at which the data for this section will be stored
        /// </summary>
        public string dataPath;

        /// <summary>
        /// The identifier with which this section will be accessible
        /// </summary>
        public string sectionID;

        private void Reset()
        {
            dataPath = Path.Combine(
                CsSettings.hasInstance ? Path.GetDirectoryName(CsSettings.singleton.dataSavingPath) : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Application.companyName, Application.productName, "Data"), Guid.NewGuid().ToString() + ".bin");
        }
    }
}