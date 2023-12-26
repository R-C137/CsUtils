/* GameData.cs - C's Utils
 * 
 * Stores arbitrary data persistently, based on Unity's PlayerPrefs
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
 */
using CsUtils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

public class GameData : Singleton<GameData>
{
    /// <summary>
    /// All of the data available
    /// </summary>
    public Dictionary<string, object> data = new();

    /// <summary>
    /// Whether the game data has been loaded
    /// </summary>
    bool dataLoaded;

    protected override void Awake()
    {
        base.Awake();
        LoadData();
    }

    /// <summary>
    /// Loads all the saved data from disk
    /// </summary>
    void LoadData()
    {
        if (!Directory.Exists(Path.GetDirectoryName(CsSettings.singleton.dataSavingPath)) || !File.Exists(CsSettings.singleton.dataSavingPath) || dataLoaded)
            return;
        
        FileStream fs = File.Open(CsSettings.singleton.dataSavingPath, FileMode.Open, FileAccess.ReadWrite);

        using StreamReader sr = new(fs);
        data = JsonConvert.DeserializeObject<Dictionary<string, object>>(sr.ReadToEnd());

        dataLoaded = true;
    }

    /// <summary>
    /// Writes all the saved data to disk
    /// </summary>
    void SaveData()
    {
        using StreamWriter wr = new(GetDataFileStream());
        string json = JsonConvert.SerializeObject(data);
        wr.Write(json);

        static FileStream GetDataFileStream()
        {
            if (!Directory.Exists(Path.GetDirectoryName(CsSettings.singleton.dataSavingPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(CsSettings.singleton.dataSavingPath));

            if (File.Exists(CsSettings.singleton.dataSavingPath))
                return File.Open(CsSettings.singleton.dataSavingPath, FileMode.Create, FileAccess.ReadWrite);

            return File.Open(CsSettings.singleton.dataSavingPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }
    }

    /// <summary>
    /// Gets a persistent data
    /// </summary>
    /// <typeparam name="T">The type of the data</typeparam>
    /// <param name="id">The id associated with the data</param>
    /// <param name="defaultValue">The default value to return is no data was saved</param>
    /// <returns>The value of the data with the associated id</returns>

    public static T Get<T>(string id, T defaultValue = default)
    {
        singleton.LoadData();

        if (singleton.data.TryGetValue(id, out object value))
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
    public static T Set<T>(string id, T value)
    {
        singleton.data[id] = value;

        singleton.SaveData();

        return value;
    }

    /// <summary>
    /// Whether a persistent data exits
    /// </summary>
    /// <param name="id">The id of the data to check</param>
    /// <returns>Whether the persistent data exists</returns>
    public static bool Has(string id)
    {
        singleton.LoadData();

        return singleton.data.ContainsKey(id);
    }
}
