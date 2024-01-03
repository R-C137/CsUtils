/* PersistentData.cs - C's Utils
 * 
 * Stores arbitrary data persistently, divisible into multiple sections for better performance
 * 
 * NOTE: Setting & getting data with type 'float' may result in an invalid cast exception. Please instead store them as 'double'
 * 
 * Creation Date: 03/01/2024
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [03/01/2024] - Initial implementation (C137)
 */
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

public class PersistentData
{
    /// <summary>
    /// The path at which data for this section will be stored
    /// </summary>
    public string dataPath;

#pragma warning disable IDE1006 // Naming Styles
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
#pragma warning restore IDE1006 // Naming Styles

    /// <summary>
    /// Whether all of the data has been loaded
    /// </summary>
    bool dataLoaded;

    public PersistentData(string dataPath, bool loadData = true)
    {
        this.dataPath = dataPath;

        if(loadData)
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
            return (T)value;

        return defaultValue;
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
        //Prevents serializing all of the data unnecessarily
        if (data.TryGetValue(id, out object previousValue) && previousValue == (object)value)
            return value;

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
    /// Loads all of the saved data from disk
    /// </summary>W
    public void LoadData(bool forced = false)
    {
        if ((!forced && dataLoaded) || !Directory.Exists(Path.GetDirectoryName(dataPath)) || !File.Exists(dataPath))
            return;

        FileStream fs = File.Open(dataPath, FileMode.Open, FileAccess.ReadWrite);

        using StreamReader sr = new(fs);
        data = JsonConvert.DeserializeObject<Dictionary<string, object>>(sr.ReadToEnd());

        dataLoaded = true;
    }

    /// <summary>
    /// Writes all of the saved data to disk
    /// </summary>
    public void SaveData()
    {
        using StreamWriter wr = new(GetDataFileStream());
        string json = JsonConvert.SerializeObject(data);
        wr.Write(json);

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
