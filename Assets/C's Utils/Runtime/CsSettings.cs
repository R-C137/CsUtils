/* CsSettings.cs - C's Utils
 * 
 * A class containing the general settings for C's utilities. System specific settings will be found in their appropriate scripts
 * 
 * NOTE: Some settings may not be serialized in the inspector and may need to be set with script
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
 */
using CsUtils.Systems.Logging;
using System;
using System.IO;

namespace CsUtils
{
    public class CsSettings : Singleton<CsSettings>
    {
        /// <summary>
        /// What logger should C's Utilities use
        /// </summary>
        public ILogger logger;

        /// <summary>
        /// Where should the logging folder be found
        /// </summary>
        public string loggingFilePath;

        /// <summary>
        /// Where should all of the persistent game data be saved
        /// </summary>
        public string dataSavingPath;

        /// <summary>
        /// Shortcut to access the logger
        /// </summary>
        public static ILogger Logger => singleton.logger;

        protected override void Awake()
        {
            base.Awake();

            logger ??= Logging.singleton;
        }

        private void Reset()
        {
            loggingFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), UnityEngine.Application.companyName, UnityEngine.Application.productName, "Logging", "latest.log");
            dataSavingPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), UnityEngine.Application.companyName, UnityEngine.Application.productName, "Data", "Persistent Data.bin");
        }
    }
}
