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
 */

using CsUtils.Systems.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace CsUtils.Systems.DataSaving
{
    [UnityEngine.DefaultExecutionOrder(-20), UnityEngine.ExecuteAlways]
    public class GameData : Singleton<GameData>
    {
        /// <summary>
        /// The different sections in which data is saved
        /// </summary>
        public DataSectionSO[] dataSections;

        /// <summary>
        /// The actual sections in which data is stored
        /// </summary>
        public static Dictionary<string, PersistentData> persistenDataSections = new();

#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// Event raised when the value of a persistent data from the default data section is updated
        /// </summary>
        public static event PersistentData.DataUpdated onDataUpdated
        {
            add => persistenDataSections["default"].onDataUpdated += value;
            remove => persistenDataSections["default"].onDataUpdated -= value;
        }
#pragma warning restore IDE1006 // Naming Styles

        protected override void Awake()
        {
            base.Awake();

            SetupDataSections();
        }

        //Properly setups the data sections and handles clashing
        void SetupDataSections()
        {
            //Add the base path data saving path as the default one
            persistenDataSections.Clear();

            if (CsSettings.hasInstance)
                persistenDataSections.Add("default", new PersistentData(CsSettings.singleton.DataSavingPath));
            else
                StaticUtils.AutoLog("No instance of 'CsSettings' was found. Default path could not be added", LogSeverity.Warning);

            //Check for clashing section ids and paths
            if (!ClashCheck())
                return;

            //Initiate new data sections
            foreach (var section in dataSections.Where(s => s != null))
            {
                persistenDataSections.Add(section.sectionID, new PersistentData(section.dataPath));
            }
        }

        /// <summary>
        /// Checks for clashing section ids and paths
        /// </summary>
        bool ClashCheck()
        {
            HashSet<string> sectionIDs = new();
            HashSet<string> sectionsPaths = new();

            List<string> clashingIDs = new();
            List<string> clashingPaths = new();

            foreach (var section in dataSections.Where(s => s != null))
            {
                if (!sectionIDs.Add(section.sectionID))
                {
                    if (!sectionsPaths.Add(section.dataPath))
                    {
                        //If both section id and path are clashing, we can safely remove one of them
                        persistenDataSections.Remove(section.sectionID);
                        goto defaultClashCheck;
                    }
                    clashingIDs.Add(section.sectionID);
                    goto defaultClashCheck;
                }

                if (!sectionsPaths.Add(section.dataPath))
                {
                    clashingPaths.Add(section.dataPath);
                }

                defaultClashCheck:
                if (section.dataPath == persistenDataSections["default"].dataPath)
                    StaticUtils.AutoLog("Data section with id {0} cannot be set to the default persistent data path", LogSeverity.Warning, parameters: section.sectionID);
            }

            if (clashingIDs.Any())
                StaticUtils.AutoLog("Found clashing ids for data saving {0}, " + (Application.isPlaying ? "removing script" : "script will be removed at runtime"), LogSeverity.Warning, gameObject, parameters: sectionIDs);

            if (clashingPaths.Any())
                StaticUtils.AutoLog("Found clashing paths for data saving {0}, " + (Application.isPlaying ? "removing script" : "script will be removed at runtime"), LogSeverity.Error, gameObject, parameters: sectionIDs);

            if (clashingIDs.Any() || clashingPaths.Any())
            {
                if(Application.isPlaying)
                    Destroy(this);

                return false;
            }

            return true;
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
            if (persistenDataSections.TryGetValue(sectionID, out PersistentData section))
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
            if (persistenDataSections.TryGetValue(sectionID, out PersistentData section))
                return section.TryGet(id, out value);

            throw new ArgumentException("The specified section doesn't exist", nameof(sectionID));
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
            if (persistenDataSections.TryGetValue(sectionID, out PersistentData section))
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
            if (persistenDataSections.TryGetValue(sectionID, out PersistentData section))
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
            if (persistenDataSections.TryGetValue(sectionID, out PersistentData section))
                section.Clear(id);

            throw new ArgumentException("The specified section doesn't exist", nameof(sectionID));
        }

        /// <summary>
        /// Clears all the data of a section
        /// </summary>
        /// <param name="sectionID">The id of the section whose data should be reset</param>
        public void Clear(string sectionID = "default")
        {
            if (persistenDataSections.TryGetValue(sectionID, out PersistentData section))
                section.ClearAll();

            throw new ArgumentException("The specified section doesn't exist", nameof(sectionID));
        }

        /// <summary>
        /// Clear all of the data for every section
        /// </summary>
        public void ClearAll()
        {
            foreach (var section in persistenDataSections.Values)
            {
                section.ClearAll();
            }
        }

        /// <summary>
        /// Fixes any type casting errors caused by the json de-serialization
        /// </summary>
        /// <typeparam name="T">The type that the value should be casted to</typeparam>
        /// <param name="value">The value to fix the casting of</param>
        /// <param name="destination">The value casted to its correct typet</param>
        /// <returns>Whether a fix was applied</returns>
        public static bool FixTypeCasting<T>(object value, out T destination)
        {
            if (typeof(T) == typeof(float) && value is double)
            {
                destination = (T)Convert.ChangeType(value, typeof(T));
                return true;
            }

            destination = (T)value;
            return false;
        }

        private void OnValidate()
        {
            SetupDataSections();
        }
    }
}
