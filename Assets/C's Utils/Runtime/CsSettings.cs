/* CsSettings.cs - C's Utils
 * 
 * A class containing the general settings for C's utilities. System specific settings will be found in their appropriate scripts
 * 
 * NOTE: Some settings may not be serialized in the inspector and may need to be set with a script
 * 
 * Creation Date: 03/12/2023
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [03/12/2023] - Initial implementation (C137)
 *                   - Logging system access shortcut (C137)
 *                   - Fixed null values always populating (C137)
 *                   - Logging folder path support (C137)
 *                   
 *      [25/12/2023] - Better default values (C137)
 *      [03/01/2024] - Class is now also ran in the Editor (C137)
 *      [16/04/2024] - Added a reference to the default modal window prefab (C137)
 *      [17/04/2024] - Added support for environment variables in file paths (C137)
 *      [29/04/2024] - Fixed data saving path default value (C137)
 *      [12/05/2024] - Full data path is now always returned
 *      [19/07/2024] - Logger is no longer assgined in 'CsSettings' (C137)
 *      [22/07/2024] - Proper singleton implementation (C137)
 *                   - Updated execution order (C137)
 *
 *      [31/07/2024] - Added support for the context menu (C137)
 *      [05/08/2024] - Added support for a C's Utils Gameobject (C137)
 *      
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using ILogger = CsUtils.Systems.Logging.ILogger;

namespace CsUtils
{
    [UnityEngine.DefaultExecutionOrder(-40), UnityEngine.ExecuteAlways]
    public class CsSettings : UnityEngine.MonoBehaviour
    {
        [Serializable]
        public struct ContextMenuData
        {
            /// <summary>
            /// The actual prefab containing the context menu
            /// </summary>
            public UnityEngine.GameObject contextMenuPrefab;

            /// <summary>
            /// Prefab for each option
            /// </summary>
            public UnityEngine.GameObject optionPrefab;
        }
        
        /// <summary>
        /// What logger should C's Utilities use
        /// </summary>
        public ILogger logger;

        /// <summary>
        /// Reference to the gameobject used to hold required monobehaviour for static classes
        /// </summary>
        public static GameObject csUtilsGameobject;

        /// <summary>
        /// The prefab to use for the modal window
        /// </summary>
        public UnityEngine.GameObject modalWindowPrefab;

        /// <summary>
        /// The prefab to use for the context menu
        /// </summary>
        public ContextMenuData contextMenuData;
        
        /// <summary>
        /// Where should the logging folder be found
        /// </summary>
        [UnityEngine.SerializeField]
        string loggingFilePath;

        /// <summary>
        /// Where should all of the persistent game data be saved
        /// </summary>
        [UnityEngine.SerializeField]
        string dataSavingPath;

        /// <summary>
        /// The full path of the logging file path
        /// </summary>
        public string LoggingFilePath => Environment.ExpandEnvironmentVariables(ReplaceCustomDefinitions(loggingFilePath));
        
        /// <summary>
        /// The full file path of the data saving path
        /// </summary>
        public string DataSavingPath => Environment.ExpandEnvironmentVariables(ReplaceCustomDefinitions(dataSavingPath));

        /// <summary>
        /// Shortcut to access the logger
        /// </summary>
        public static ILogger Logger => Singleton.Get<CsSettings>().logger;


        private void Awake()
        {
            Singleton.Create(this);
        }

        /// <summary>
        /// Replace file path by custom definitions
        /// </summary>
        /// <param name="path">The original file path</param>
        /// <returns>The file path with the customs definitions added</returns>
        public string ReplaceCustomDefinitions(string path)
        {
            Dictionary<string, string> definitions = new()
            {
                { "%unity.companyName%", UnityEngine.Application.companyName },
                { "%unity.productName%", UnityEngine.Application.productName }
            };

            foreach(var definition in definitions)
            {
                path = Regex.Replace(path, definition.Key, definition.Value, RegexOptions.IgnoreCase);
            }

            return path;
        }

        private void Reset()
        {
            loggingFilePath = Path.Combine("%appdata%", "%unity.companyName%", "%unity.productName%", "Logging", "latest.log");
            dataSavingPath = Path.Combine("%appdata%", "%unity.companyName%", "%unity.productName%", "Data", "Persistent Data.bin");
        }

        private void OnDestroy()
        {
            Singleton.Remove(this);
        }
    }
}
