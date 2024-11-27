/* PersistentData.cs - C's Utils
 * 
 * Stores arbitrary data persistently, divisible into multiple sections for better performance
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
 *      [04/01/2024] - Added TryGet<T>() support (C137)
 *      [05/01/2024] - Fixed 'float' and 'double' casting errors (C137)
 *                   - Added data removal support (C137)
 *                   - Added missing namespace (C137)
 *
 *      [23/11/2024] - Added support for data obfuscation (C137)
 *      [24/11/2024] - Added methods for getting & settings raw file data (C137)
 *      [27/11/2024] - Removed data change checks (C137)
 * 
 */
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CsUtils.Systems.DataSaving
{
    /// <summary>
    /// Base interface for obfuscating data
    /// </summary>
    public interface IDataObfuscator
    {
        /// <summary>
        /// Obfuscates json data before it is saved by the data-saving system
        /// </summary>
        /// <param name="jsonData">The json data to obfuscate</param>
        /// <returns>The obfuscated json data</returns>
        public byte[] Obfuscate(string jsonData);
        
        //// <summary>
        /// Deobfuscates json data before it is loaded by the data-saving system
        /// </summary>
        /// <param name="obfuscatedJsonData">The json data to deobfuscate</param>
        /// <returns>The deobfuscated json data</returns>
        public string DeObfuscate(byte[] obfuscatedJsonData);
    }
    
    public class PersistentData
    {
        /// <summary>
        /// The path at which data for this section will be stored
        /// </summary>
        public string dataPath;

        /// <summary>
        /// The obfuscator logic to apply to data for this section
        /// </summary>
        public IDataObfuscator dataObfuscator;
        
        /// <summary>
        /// All of the data available
        /// </summary>
        public Dictionary<string, object> data { get; private set; } = new();

        /// <summary>
        /// Event raised when the value of a data is updated
        /// </summary>
        /// <param name="id">The id of the data that was updated</param>
        /// <param name="data">The new value of the data</param>
        public delegate void DataUpdated(string id, object data);
        public event DataUpdated onDataUpdated;

        /// <summary>
        /// Whether all of the data has been loaded
        /// </summary>
        bool dataLoaded;

        public PersistentData(string dataPath, bool loadData = true, IDataObfuscator dataObfuscator = null)
        {
            this.dataPath = dataPath;
            this.dataObfuscator = dataObfuscator;
            
            if (loadData)
                LoadData();
        }

        /// <summary>
        /// Gets a persistent data
        /// </summary>
        /// <typeparam name="T">The type of the data</typeparam>
        /// <param name="id">The id associated with the data</param>
        /// <param name="defaultValue">The default value to return is no data was saved</param>
        /// <returns>The value of the data with the associated id</returns>
        public T Get<T>(string id, T defaultValue = default)
        {
            LoadData();

            if (data.TryGetValue(id, out object value))
            {
                GameData.FixTypeCasting(value, out T castedData);
                return castedData;
            }

            return defaultValue;
        }

        /// <summary>
        /// Tries to get a persistent data
        /// </summary>
        /// <typeparam name="T">The type of the ud</typeparam>
        /// <param name="id">The id associated with the data</param>
        /// <param name="value">The value of the data with the associated id if it exists</param>
        /// <returns>Whether data with the associated id exists</returns>
        public bool TryGet<T>(string id, out T value)
        {
            bool result = data.TryGetValue(id, out object _value);

            if (_value != null)
                GameData.FixTypeCasting(_value, out value);
            else
                value = default;

            return result;
        }

        /// <summary>
        /// Sets a persistent data
        /// </summary>
        /// <typeparam name="T">The type of the data</typeparam>
        /// <param name="id">The id associated with the data</param>
        /// <param name="value">The value to save the data with</param>
        /// <returns>The data that was saved</returns>
        public T Set<T>(string id, T value)
        {
            data[id] = value;

            SaveData();

            onDataUpdated?.Invoke(id, value);

            return value;
        }

        /// <summary>
        /// Whether a persistent data exits
        /// </summary>
        /// <param name="id">The id of the data to check</param>
        /// <returns>Whether the persistent data exists</returns>
        public bool Has(string id)
        {
            LoadData();

            return data.ContainsKey(id);
        }

        /// <summary>
        /// Clears the saved value for a persistent data
        /// </summary>
        /// <param name="id">The id of the persistent data to removed</param>
        public void Clear(string id)
        {
            LoadData();

            if (data.ContainsKey(id))
            {
                data.Remove(id);
                SaveData();
            }
        }

        /// <summary>
        /// Clears all saved data
        /// </summary>
        public void ClearAll()
        {
            LoadData();
            data.Clear();
            SaveData();
        }

        /// <summary>
        /// Returns the raw bytes saved to this section
        /// </summary>
        /// <param name="fileStream">The filestream to use</param>
        /// <returns></returns>
        public byte[] GetRawData(FileStream fileStream = null)
        {
            bool closeStream = fileStream == null;
            
            fileStream ??= File.Open(dataPath, FileMode.Open, FileAccess.ReadWrite);

            using MemoryStream memoryStream = new ();
            
            fileStream.CopyTo(memoryStream);  // Copy all bytes from fileStream to memoryStream
            
            if(closeStream)
                fileStream.Close();
            
            return memoryStream.ToArray();    // Convert the memoryStream to byte array
            
        }
        
        /// <summary>
        /// Loads all of the saved data from disk
        /// </summary>W
        public void LoadData(bool forced = false)
        {
            if ((!forced && dataLoaded) || !Directory.Exists(Path.GetDirectoryName(dataPath)) || !File.Exists(dataPath))
                return;

            using FileStream fs = File.Open(dataPath, FileMode.Open, FileAccess.ReadWrite);

            byte[] obfuscatedJsonData = GetRawData(fs);
            data = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataObfuscator.DeObfuscate(obfuscatedJsonData));
            
            dataLoaded = true;
        }

        /// <summary>
        /// Writes all of the saved data to disk
        /// </summary>
        public void SaveData()
        {
            string json = JsonConvert.SerializeObject(data);
            SaveRawData(dataObfuscator.Obfuscate(json));
        }

        /// <summary>
        /// Saves raw bytes to data of this section
        /// </summary>
        /// <param name="saveData">The bytes to save</param>
        /// <param name="fileStream">The filestream to use</param>
        public void SaveRawData(byte[] saveData, FileStream fileStream = null)
        {
            bool closeStream = fileStream == null;
            
            fileStream ??= GetDataFileStream();
            
            fileStream.Write(saveData);
            
            if (closeStream)
                fileStream.Close();
            
            FileStream GetDataFileStream()
            {
                if (!Directory.Exists(Path.GetDirectoryName(dataPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(dataPath));

                if (File.Exists(dataPath))
                    return File.Open(dataPath, FileMode.Create, FileAccess.ReadWrite);

                return File.Open(dataPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            }
        }
    }
}