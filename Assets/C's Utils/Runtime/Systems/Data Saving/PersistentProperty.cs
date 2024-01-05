/* PersistentProperty.cs - C's Utils
 * 
 * Allows any property to be persistent across sessions
 *
 * NOTE: Needs to be initialized in Awake() or it will throw errors 
 * 
 * Creation Date: 04/01/2024
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [04/01/2024] - Initial implementation (C137)
 *      [05/01/2024] - Added missing namespace (C137)
 */
namespace CsUtils.Systems.DataSaving
{
    public class PersistentProperty<T>
    {
        /// <summary>
        /// The section where this property will be stored
        /// </summary>
        public string sectionID;

        /// <summary>
        /// The id with which this data will be saved
        /// </summary>
        public string dataID;

        /// <summary>
        /// Accessor for the data
        /// </summary>
        //// Yes this property is in PascalCase. It's to be closer to the syntax used with Nullable types
        public T Value
        {
            get
            {
                return _data;
            }
            set
            {
                GameData.Set(dataID, value, sectionID);
            }
        }

        /// <summary>
        /// The cached value of the data
        /// </summary>
        T _data;

        /// <summary>
        /// Instantiates a new persistent property 
        /// </summary>
        /// <param name="dataID">The id of the data associated with this persistent property</param>
        /// <param name="defaultValue">The default value of this persistent property</param>
        /// <param name="sectionID">The id of the section associated with this persistent property</param>
        public PersistentProperty(string dataID, T defaultValue = default, string sectionID = "default")
        {
            this.sectionID = sectionID;
            this.dataID = dataID;

            UpdateData(defaultValue);
            GameData.persistenDataSections[sectionID].onDataUpdated += DataUpdated;
        }

        ~PersistentProperty()
        {
            GameData.persistenDataSections[sectionID].onDataUpdated -= DataUpdated;
        }

        /// <summary>
        /// Called when data on the section of this data is updated
        /// </summary>
        /// <param name="id">The id of the data that was updated</param>
        /// <param name="data">The new value of the data</param>
        private void DataUpdated(string id, object data)
        {
            if (id == dataID)
                _data = (T)data;
        }

        /// <summary>
        /// Manually updates the cached value of the data
        /// </summary>
        public void UpdateData(T defaultValue = default)
        {
            _data = GameData.Get(dataID, sectionID, defaultValue);
        }
    }
}
