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
 */
using CsUtils;
using CsUtils.Systems.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using static PersistentData;

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
    public static event DataUpdated onDataUpdated
    {
        add => persistenDataSections["default"].onDataUpdated += value;
        remove => persistenDataSections["default"].onDataUpdated -= value;
    }
#pragma warning restore IDE1006 // Naming Styles

    protected override void Awake()
    {
        base.Awake();

        //Add the base path data saving path as the default one
        persistenDataSections.Add("default", new PersistentData(CsSettings.singleton.dataSavingPath));

        //Check for clashing section ids and paths
        if (!ClashCheck())
            return;

        //Initiate new data sections
        foreach (var section in dataSections)
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

        foreach (var section in dataSections.ToArray())
        {
            if (!sectionIDs.Add(section.sectionID))
            {
                if (!sectionsPaths.Add(section.dataPath))
                {
                    //If both section id and path are clashing, we can safely remove one of them
                    persistenDataSections.Remove(section.sectionID);
                    continue;
                }
                clashingIDs.Add(section.sectionID);
                continue;
            }

            if (!sectionsPaths.Add(section.dataPath))
            {
                clashingPaths.Add(section.dataPath);
            }

            if (section.dataPath == persistenDataSections["default"].dataPath)
                CsSettings.Logger.LogDirect("Data section with id {0} cannot be set to the default persistent data path", LogSeverity.Warning, parameters: section.sectionID);
        }

        if(clashingIDs.Any())
            CsSettings.Logger.LogDirect("Found clashing ids for data saving {0}, removing script", LogSeverity.Warning, gameObject, parameters: sectionIDs);

        if(clashingPaths.Any())
            CsSettings.Logger.LogDirect("Found clashing paths for data saving {0}, removing script", LogSeverity.Error, gameObject, parameters: sectionIDs);

        if (clashingIDs.Any() || clashingPaths.Any())
        {
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
        if(persistenDataSections.TryGetValue(sectionID, out PersistentData section))
            return section.Get(id, defaultValue);

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
}
