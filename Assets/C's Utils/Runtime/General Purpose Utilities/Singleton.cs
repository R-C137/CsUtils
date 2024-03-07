/* Singleton.cs - C's Utils
 * 
 * Makes the derived class a singleton. Will create a new instance of such class upon being accessed if no previous instance is available.
 * 
 * NOTE: "base.Awake()" needs to be called to ensure that the singleton is properly registered or disposed of.
 * 
 * 
 * Creation Date: 28/11/2023
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [28/11/2023] - Initial implementation (C137)
 *      [03/12/2023] - Log system support (C137)
 *                   - Fixed StackOverflow error with new log system support (C137)
 *      
 *      [25/12/2023] - Added a property to check if an instance of the singleton exists (C137)
 *                   - Improved singleton instance creation (C137)
 *                   
 *      [03/01/2023] - Logging is now done with the default unity logging system (C137)
 *      [07/03/2023] - Updated deprecated method calls to new ones (C137)
 *                   - Added a couple changes to metadata (C137)
 *      
 */
using UnityEngine;

namespace CsUtils
{
    /// <summary>
    /// Makes the derived class a singleton. Will create a new instance of such class upon being accessed if no previous instance is available.
    /// </summary>
    /// <typeparam name="T">The Unity component to turn into a singleton</typeparam>
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        /// <summary>
        /// The cached instance of the singleton
        /// </summary>
        private static T instance;

#pragma warning disable IDE1006 // Naming Styles

        /// <summary>
        /// Access to the instance of the singleton. Will create one if no instance is found
        /// </summary>
        public static T singleton
        {
            get
            {
                // Create a new instance of T if none is currently present
                if ((instance ??= FindFirstObjectByType<T>()) == null)
                {
                    if (!Application.isPlaying)
                    {
                        Debug.LogWarning($"No instance of '{typeof(T).Name}' has been found. None will be created automatically as the singleton isn't being accessed at runtime");
                        throw new System.NullReferenceException("No instance of the singleton was found");
                    }

                    var obj = new GameObject($"{typeof(T).Name} Singleton");

                    instance = obj.AddComponent<T>();

                    Debug.LogWarning($"No instance of '{typeof(T).Name}' has been found. One has been created automatically", obj);
                }

                return instance;
            }
        }

        /// <summary>
        /// Whether an instance of the singleton currently exists
        /// </summary>
        public static bool hasInstance
        {
            get
            {
                return (instance ??= FindFirstObjectByType<T>()) == null;
            }
        }
#pragma warning restore IDE1006 // Naming Styles

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
            }
            else if (instance != this as T)
            {
                //Destroy component if an instance already exists
                Destroy(this);

                CsSettings.Logger.LogDirect($"Two or more instances of '{typeof(T).Name}' exist, a singleton should only have one instance. '{gameObject.name}.{typeof(T).Name}' has been destroyed",Systems.Logging.LogSeverity.Warning, gameObject);
            }
        }
    }
}