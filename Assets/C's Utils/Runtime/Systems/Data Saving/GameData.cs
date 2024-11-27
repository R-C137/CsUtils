/* GameData.cs - C's Utils
 * 
 * Stores arbitrary data persistently in multiple sections, based on Unity's PlayerPrefs
 * 
 * 
 * Creation Date: 26/12/2023
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [26/12/2023] - Initial implementation (C137)
 *      [01/01/2024] - Added an event that is raised when the value of a data is updated (C137)
 *      [03/01/2024] - Data sectioning is now supported (C137)
 *                   - Methods and fields are now static for ease of access (C137)
 *                   
 *      [04/01/2024] - Added TryGet<T>() support (C137)
 *                   - Updated default execution order (C137)
 *      
 *      [05/01/2024] - Added data removal support (C137)
 *                   - Added missing namespace (C137)
 *                   - Updated execution order for editor data saving support (C137)
 *                   
 *      [19/07/2024] - Fixed unhandled exception at creation (C137)
 *                   - Improved clash checks (C137)
 *                   - Clash check is now done everytime data sections are modified (C137)
 *                   
 *      [22/07/2024] - Proper singleton implementation (C137)
 *      [22/11/2024] - Improved clash handling (C137)
 *                   - Fixed NullReferenceException on awake (C137)
 *                   - Added verbosity to clashing errors (C137)
 *                   - Singleton values are now properly set in the editor (C137)
 *
 *      [23/11/2024] - Added support for saving scriptable objects (C137)
 *                   - Fixed TryGet() throwing errors (C137)
 *                   - Added support for data obfuscation (C137)
 *
 *      [24/11/2024] - Fixed typo in a field name (C137)
 *      [27/11/2024] - Improved logging of clashing sections (C137)
 * 
 */

using CsUtils.Systems.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace CsUtils.Systems.DataSaving
{
    
    [UnityEngine.DefaultExecutionOrder(-20), UnityEngine.ExecuteAlways]
    public class GameData : MonoBehaviour
    {
        /// <summary>
        /// The different sections in which data is saved
        /// </summary>
        public List<DataSectionSo> dataSections = new();

        /// <summary>
        /// The actual sections in which data is stored
        /// </summary>
        public static Dictionary<string, PersistentData> persistentDataSections = new();

        /// <summary>
        /// The default obfuscator logic to use for all data sections
        /// </summary>
        public DataObfuscatorSo defaultObfuscator;
        
        /// <summary>
        /// Event raised when the value of a persistent data from the default data section is updated
        /// </summary>
        public static event PersistentData.DataUpdated onDataUpdated
        {
            add => persistentDataSections["default"].onDataUpdated += value;
            remove => persistentDataSections["default"].onDataUpdated -= value;
        }

        void Awake()
        {
            Singleton.Create(this);

            SetupDataSections();
        }
        
        private void Update()
        {
            // Recreate singleton in case assemblies were recompiled
            if(!Application.isPlaying)
                Singleton.Create(this);
        }

        //Properly setups the data sections and handles clashing
        void SetupDataSections()
        {
            //Add the base path data saving path as the default one
            persistentDataSections.Clear();

            Singleton.TryGet(out CsSettings csSettings);
            
            if (csSettings != null)
                persistentDataSections.Add("default", new PersistentData(csSettings.dataSavingFilePath, dataObfuscator: defaultObfuscator));
            else
                StaticUtils.AutoLog("No instance of 'CsSettings' was found. Default path could not be added", LogSeverity.Warning);

            //Check for clashing section ids and paths
            if (ClashCheck())
                return;

            //Initiate new data sections
            foreach (var section in dataSections.Where(s => s != null))
            {
                IDataObfuscator obfuscator = section.dataObfuscator == null ? defaultObfuscator : section.dataObfuscator;
                
                persistentDataSections.Add(section.sectionID, new PersistentData(section.dataPath, dataObfuscator: obfuscator));
            }
        }

        /// <summary>
        /// Checks for clashing section ids and paths. Will attempt to remove duplicates if found to minimise clashes
        /// </summary>
        /// <returns>Whether there were any clashes that couldn't be automatically resolved</returns>
        bool ClashCheck()
        {
            HashSet<DataSectionSo> currentDataSections = new();

            HashSet<string> currentSectionIds = new();
            HashSet<string> currentDataPaths = new();

            Dictionary<string, List<string>> clashingPaths = new();
            Dictionary<string, List<string>> clashingIds = new();
            
            bool iDsClashed = false;
            bool pathsClashed = false;
            
            foreach (DataSectionSo section in dataSections.Where(d => d != null).ToArray())
            {
                if(!currentDataSections.Add(section))
                {
                    // Remove duplicates
                    dataSections[dataSections.LastIndexOf(section)] = null;
                    StaticUtils.AutoLog("A duplicate data section was found and removed. No duplicates are allowed as data sections", LogSeverity.Info, this);
                    continue;
                }

                if(!currentSectionIds.Add(section.sectionID))
                {
                    if(currentDataPaths.Contains(section.dataPath))
                    {
                        dataSections.Remove(section); // Since IDs and paths match, we can remove this one as it is a duplicate
                        
                        StaticUtils.AutoLog("A duplicate data section was found and removed. No duplicates are allowed as data sections", LogSeverity.Info, this);
                        continue;
                    }

                    iDsClashed = true;
                    string sectionID = string.IsNullOrEmpty(section.sectionID) ? section.name : section.sectionID;

                    if(clashingIds.ContainsKey(sectionID))
                        clashingIds[sectionID].Add(section.dataPath);
                    
                    clashingIds.Add(string.IsNullOrEmpty(section.sectionID) ? section.name : section.sectionID, new List<string>{section.sectionID});
                }

                if(!currentDataPaths.Add(section.dataPath))
                {
                    pathsClashed = true;

                    string sectionID = string.IsNullOrEmpty(section.sectionID) ? section.name : section.sectionID;

                    if(clashingPaths.ContainsKey(sectionID))
                        clashingPaths[sectionID].Add(section.rawDataPath);
                    
                    clashingPaths.Add(string.IsNullOrEmpty(section.sectionID) ? section.name : section.sectionID, new List<string>{section.rawDataPath});
                }
            }

            if(iDsClashed)
            {
                List<string> idsDisplay = new();

                foreach (List<string> ids in clashingIds.Values)
                {
                    foreach (string id in ids)
                    {
                        idsDisplay.Add(id);
                    }
                }
                
                StaticUtils.AutoLog("Clashing IDs, {0}, were found from, {1}, respectively. The data saving system will not work as intended", LogSeverity.Error, parameters: new object[] { idsDisplay, clashingIds.Keys.ToArray() });
            }
            if(pathsClashed)
            {
                List<string> pathsDisplay = new();

                foreach (List<string> paths in clashingPaths.Values)
                {
                    foreach (string path in paths)
                    {
                        pathsDisplay.Add(path);
                    }
                }
                StaticUtils.AutoLog("Clashing paths, {0}, were found from, {1}, respectively. The data saving system will not work as intended", LogSeverity.Error, parameters: new object[] { pathsDisplay, clashingPaths.Keys.ToArray()});
            }

            return iDsClashed || pathsClashed;
        }

        /// <summary>
        /// Gets a persistent data
        /// </summary>
        /// <typeparam name="T">The type of the data</typeparam>
        /// <param name="id">The id associated with the data</param>
        /// <param name="sectionID">The id of the section in which to get the persistent data</param>
        /// <param name="defaultValue">The default value to return is no data was saved</param>
        /// <returns>The value of the data with the associated id</returns>
        public static T Get<T>(string id, string sectionID = "default", T defaultValue = default)
        {
            if (persistentDataSections.TryGetValue(sectionID, out PersistentData section))
                return section.Get(id, defaultValue);

            throw new ArgumentException("The specified section doesn't exist", nameof(sectionID));
        }

        /// <summary>
        /// Tries to get a persistent data
        /// </summary>
        /// <typeparam name="T">The type of the ud</typeparam>
        /// <param name="id">The id associated with the data</param>
        /// <param name="value">The value of the data with the associated id if it exists</param>
        /// <param name="sectionID">The id of the section in which to get the persistent data</param>
        /// <returns>Whether data with the associated id exists</returns>
        public static bool TryGet<T>(string id, out T value, string sectionID = "default")
        {
            if (persistentDataSections.TryGetValue(sectionID, out PersistentData section))
                return section.TryGet(id, out value);

            value = default;
            
            return false;
        }

        /// <summary>
        /// Sets a persistent data for a section
        /// </summary>
        /// <typeparam name="T">The type of the data</typeparam>
        /// <param name="id">The id associated with the data</param>
        /// <param name="value">The value to save the data with</param>
        /// <param name="sectionID">The id of the section in which to store the persistent data</param>
        /// <returns>The data that was saved</returns>
        public static T Set<T>(string id, T value, string sectionID = "default")
        {
            if (persistentDataSections.TryGetValue(sectionID, out PersistentData section))
                return section.Set(id, value);

            throw new ArgumentException("The specified section doesn't exist", nameof(sectionID));
        }

        /// <summary>
        /// Whether a persistent data exits
        /// </summary>
        /// <param name="id">The id of the data to check</param>
        /// <param name="sectionID">The id of the section in which to check for the persistent data</param>
        /// <returns>Whether the persistent data exists</returns>
        public static bool Has(string id, string sectionID = "default")
        {
            if (persistentDataSections.TryGetValue(sectionID, out PersistentData section))
                return section.Has(id);

            throw new ArgumentException("The specified section doesn't exist", nameof(sectionID));
        }

        /// <summary>
        /// Clear the saved value for a persistent data
        /// </summary>
        /// <param name="id">The id of the persistent data to removed</param>
        /// <param name="sectionID">The id of the section to remove the persistent data from</param>
        public void Clear(string id, string sectionID = "default")
        {
            if (persistentDataSections.TryGetValue(sectionID, out PersistentData section))
                section.Clear(id);

            throw new ArgumentException("The specified section doesn't exist", nameof(sectionID));
        }

        /// <summary>
        /// Clears all the data of a section
        /// </summary>
        /// <param name="sectionID">The id of the section whose data should be reset</param>
        public void Clear(string sectionID = "default")
        {
            if (persistentDataSections.TryGetValue(sectionID, out PersistentData section))
                section.ClearAll();

            throw new ArgumentException("The specified section doesn't exist", nameof(sectionID));
        }

        /// <summary>
        /// Clear all of the data for every section
        /// </summary>
        public void ClearAll()
        {
            foreach (var section in persistentDataSections.Values)
            {
                section.ClearAll();
            }
        }

        /// <summary>
        /// Fixes any type casting errors caused by the json deserialization
        /// </summary>
        /// <typeparam name="T">The type that the value should be casted to</typeparam>
        /// <param name="value">The value to fix the casting of</param>
        /// <param name="destination">The value casted to its correct typet</param>
        /// <returns>Whether a fix was applied</returns>
        public static bool FixTypeCasting<T>(object value, out T destination)
        {
            // Properly change from double to float
            if (typeof(T) == typeof(float) && value is double)
            {
                destination = (T)Convert.ChangeType(value, typeof(T));
                return true;
            }

            // Support for scriptable objects
            if(typeof(ScriptableObject).IsAssignableFrom(typeof(T)))
            {
                ScriptableObject scriptableObject = ScriptableObject.CreateInstance(typeof(T));

                JsonConvert.PopulateObject(value.ToString(), scriptableObject);

                destination = (T)Convert.ChangeType(scriptableObject, typeof(T));
                return true;
            }
            
            destination = (T)value;
            return false;
        }

        private void OnValidate()
        {
            SetupDataSections();
        }

        private void OnDestroy()
        {
            Singleton.Remove(this);
        }
    }
}
