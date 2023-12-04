/* CsSettings.cs - C's Utils
 * 
 * A class containing all the settings for C's utilities
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
        /// Shortcut to access the logger
        /// </summary>
        public static ILogger Logger => singleton.logger;

        /// <summary>
        /// Whether the null values of this class should be automatically populated with their default value
        /// </summary>
        public bool populateNullValues = true;

        protected override void Awake()
        {
            base.Awake();

            if (!populateNullValues)
                return;

            logger ??= Logging.singleton;

            if(string.IsNullOrEmpty(loggingFilePath))
                loggingFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), UnityEngine.Application.companyName, UnityEngine.Application.productName, "Logging", "latest.log");
        }
    }
}
