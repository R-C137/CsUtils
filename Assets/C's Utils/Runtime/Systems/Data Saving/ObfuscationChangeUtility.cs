/* ObfuscationChangeUtility.cs - C's Utils
 *
 * A utility class used to change the obfuscation of a data section
 *
 *
 * Creation Date: 24/11/2024
 * Authors: C137
 * Original: C137
 *
 * Edited By: C137
 *
 * Changes:
 *      [24/11/2024] - Initial implementation (C137)
 * 
 */
using System;
using System.IO;
using CsUtils.Systems.Logging;
using UnityEngine;
#if UNITY_EDITOR
using CsUtils;
using CsUtils.Systems.DataSaving;
using UnityEditor;
#endif

namespace CsUtils.Systems.DataSaving
{
    public class ObfuscationChangeUtility : MonoBehaviour
    {
        /// <summary>
        /// The target section on which to change the obfuscation
        /// </summary>
        public DataSectionSo target;
        
        /// <summary>
        /// The previous obfuscator of the target
        /// </summary>
        public DataObfuscatorSo previousObfuscator;
        
        /// <summary>
        /// The new obfuscation to set the target to
        /// </summary>
        public DataObfuscatorSo newObfuscator;

        /// <summary>
        /// Whether to also update the obfuscator of the section
        /// </summary>
        public bool autoUpdateObfuscator = true;
        
        public void ChangeTargetObfuscation()
        {
            if(target == null)
            {
                StaticUtils.AutoLog("A target needs to be specified to change obfuscation", LogSeverity.Error);
                return;
            }

            if(previousObfuscator == null)
            {
                StaticUtils.AutoLog("No previous obfuscator has been set", LogSeverity.Error);
                return;
            }

            if(newObfuscator == null)
            {
                StaticUtils.AutoLog("No new obfuscator has been set", LogSeverity.Error);
                return;
            }

            if(!File.Exists(target.dataPath))
            {
                StaticUtils.AutoLog("No obfuscated data could be found", LogSeverity.Warning);
                return;
            }

            ChangeObfuscation(target.dataPath);

            if(autoUpdateObfuscator)
            {
                ChangeSectionObfuscator();
                StaticUtils.AutoLog("Obfuscation changed successfully", LogSeverity.Info);
            }
            else 
                StaticUtils.AutoLog("Obfuscation changed successfully. Remember to update the obfuscator to the new one in the data section", LogSeverity.Info);
        }

        public void ChangeDefaultObfuscation()
        {
            if(!Singleton.TryGet(out CsSettings csSettings))
            {
                StaticUtils.AutoLog("No instance of 'CsSettings' could be found to establish the default data saving path", LogSeverity.Error);
                return;
            }
            
            if(!File.Exists(csSettings.dataSavingFilePath))
            {
                StaticUtils.AutoLog("No obfuscated data could be found", LogSeverity.Warning);
                return;
            }
            
            ChangeObfuscation(csSettings.dataSavingFilePath);
            
            if(autoUpdateObfuscator)
            {
                ChangeDefaultObfuscator();
                StaticUtils.AutoLog("Obfuscation changed successfully", LogSeverity.Info);
            }
            else 
                StaticUtils.AutoLog("Obfuscation changed successfully. Remember to update the default obfuscator", LogSeverity.Info);
        }
        
        void ChangeObfuscation(string dataPath)
        {
            PersistentData persistentData = new(dataPath, false);
            
            byte[] oldObfuscationData = persistentData.GetRawData();

            byte[] newObfuscatedData = newObfuscator.Obfuscate(previousObfuscator.DeObfuscate(oldObfuscationData));
            
            persistentData.SaveRawData(newObfuscatedData);
        }

        public void ChangeSectionObfuscator()
        {
            if(target == null)
            {
                StaticUtils.AutoLog("No target has been provided. Could not update obfuscation", LogSeverity.Error);
                return;
            }

            if(newObfuscator == null)
            {
                StaticUtils.AutoLog("No new obfuscator has been provided. Could not update obfuscation", LogSeverity.Error);
                return;
            }
            
            StaticUtils.AutoLog("Successfully set the new obfuscator for the section target", LogSeverity.Info);
        }

        public void ChangeDefaultObfuscator()
        {
            if(newObfuscator == null)
            {
                StaticUtils.AutoLog("No new obfuscator has been provided. Could not update obfuscation", LogSeverity.Error);
                return;
            }
            
            if(!Singleton.TryGet(out GameData gameData))
            {
                StaticUtils.AutoLog("No instance of 'GameData' could be found to change the default obfuscator", LogSeverity.Error);
                return;
            }
            
            gameData.defaultObfuscator = newObfuscator;
            
            StaticUtils.AutoLog("Successfully set the new default obfuscator", LogSeverity.Info);
        }
        
        private void OnValidate()
        {
            if(target != null && newObfuscator == null)
                newObfuscator = target.dataObfuscator;
        }
    }
}

#if UNITY_EDITOR


[CustomEditor(typeof(ObfuscationChangeUtility))]
public class ObfuscationChangeUtilityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        ObfuscationChangeUtility castTarget = (ObfuscationChangeUtility)target;
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("Change Section Obfuscation"))
            castTarget.ChangeTargetObfuscation();
        
        if(GUILayout.Button("Update Section Obfuscator"))
            castTarget.ChangeSectionObfuscator();
        
        GUILayout.Space(20);

        if(GUILayout.Button("Change Default Obfuscation"))
            castTarget.ChangeDefaultObfuscation();
        
        if(GUILayout.Button("Update Default Obfuscator"))
            castTarget.ChangeDefaultObfuscation();
        
        GUILayout.Space(20);
        
        if(GUILayout.Button("Reverse Obfuscators"))
            (castTarget.previousObfuscator, castTarget.newObfuscator) = (castTarget.newObfuscator, castTarget.previousObfuscator);
    }
}

#endif