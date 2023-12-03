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
 *      [12/03/2023] - Log system support (C137)
 *                   - Fixed StackOverflow error with new log system support (C137)
 */
using UnityEngine;

namespace CsUtils
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T instance;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Whilst properties should use PascalCase, I prefer using camelCase in that situation despite it being a property. It simplifies access to the singleton and ensures consistency")]
        public static T singleton
        {
            get
            {
                // Create a new instance of T if none is currently present
                if (instance == null && (instance = FindObjectOfType<T>()) == null)
                {
                    var obj = new GameObject($"{typeof(T).Name} Singleton");

                    instance = obj.AddComponent<T>();

                    CsSettings.Logger.Log($"No instance of '{typeof(T).Name}' has been found. One has been created automatically", Systems.Logging.LogLevel.Warning, obj);
                }

                return instance;

                //// Create a new instance of T if none is currently present
                //static T CreateNewInstance()
                //{ 
                //    var obj = new GameObject($"{typeof(T).Name} Singleton");

                //    CsSettings.Logger.Log($"No instance of '{typeof(T).Name}' has been found. One has been created automatically", Systems.Logging.LogLevel.Warning, obj);

                //    return obj.AddComponent<T>();
                //}
            }
        }

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

                CsSettings.Logger.Log($"Two or more instances of '{typeof(T).Name}' exist, a singleton should only have one instance. '{gameObject.name}.{typeof(T).Name}' has been destroyed",Systems.Logging.LogLevel.Warning, gameObject);
            }
        }
    }
}