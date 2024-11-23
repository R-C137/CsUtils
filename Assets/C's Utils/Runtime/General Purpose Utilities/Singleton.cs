/* Singleton.cs - C's Utils
 * 
 * Marks unity components as singletons so that their instances can be accessed statically
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
 *      [03/01/2024] - Logging is now done with the default unity logging system (C137)
 *      [07/03/2024] - Updated deprecated method calls to new ones (C137)
 *                   - Added a couple changes to metadata (C137)
 *      
 *      [19/07/2024] - Fixed 'hasInstance' returning an inverted bool (C137)
 *                   - Singleton pattern is no longer done used inheritance (C137)
 *                   
 *      [22/07/2024] - No logs are printed when trying to remove a non-singleton instance (C137)
 *      [22/11/2024] - Removed use of hash set (C137)
 *                   - Adding the same instance as a singleton will no longer remove it (C137)
 *
 *      [23/11/2024] - Singletons can now be cleared (C137)
 *                   - Added TryGet() method (C137)
 *      
 */
using CsUtils.Systems.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CsUtils
{
    public static class Singleton
    {
        static Dictionary<Type, Component> singletonInstances = new();

        /// <summary>
        /// Declares a class as a singleton. Should be done on Awake()<br></br>
        /// If another singleton of the same type has already been declared, 'instance' will be destroyed
        /// </summary>
        /// <typeparam name="T">The type of the singleton to declare</typeparam>
        /// <param name="instance">The instance of the singleton</param>
        /// <returns>Whether the singleton was successfully created</returns>
        public static bool Create<T>(T instance) where T : Component
        {
            if(!singletonInstances.TryAdd(typeof(T), instance))
            {
                if(singletonInstances[typeof(T)] == instance)
                    return true; // Since we're adding the same instance, it was already successfully created

                if(Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(instance);
                    StaticUtils.AutoLog($"Two or more instances of '{typeof(T).Name}' exist, a singleton should only have one instance. '{instance.gameObject.name}.{typeof(T).Name}' has been destroyed",
                        LogSeverity.Warning, instance.gameObject);
                }
                else
                {
                    StaticUtils.AutoLog($"Two or more instances of '{typeof(T).Name}' exist, a singleton should only have one instance. '{instance.gameObject.name}.{typeof(T).Name}' should be removed manually",
                        LogSeverity.Warning, instance.gameObject);
                }
                return false;
            }    


            return true;
        }

        /// <summary>
        /// Gets a singleton for a given type<br></br>
        /// Will search for the singleton if it was not declared (This behaviour only searches for the first item and cannot guarantee the singleton pattern)<br></br>
        /// Will create a new instance of the singleton is none was found
        /// </summary>
        /// <typeparam name="T">The type of the singleton</typeparam>
        /// <returns></returns>
        public static T Get<T>() where T : Component
        {
            if (!HasInstance<T>())
            {
                return SearchOrCreateInstance<T>();
            }

            return (T)singletonInstances[typeof(T)];
        }

        /// <summary>
        /// Gets a singleton for a given type<br></br>
        /// Will search for the singleton if it was not declared (This behaviour only searches for the first item and cannot guarantee the singleton pattern)<br></br>
        /// Will create a new instance of the singleton is none was found
        /// </summary>
        /// <param name="instance">The found instance of the singleton if any</param>
        /// <typeparam name="T">The type of the singleton</typeparam>
        /// <returns>Whether a singleton has been found or created</returns>
        public static bool TryGet<T>(out T instance) where T : Component
        {
            if (!HasInstance<T>())
            {
                instance = SearchOrCreateInstance<T>(false);

                return instance != null;
            }

            instance = (T)singletonInstances[typeof(T)];

            return true;
        }
        
        /// <summary>
        /// Removes a singleton from the declared list of singletons
        /// </summary>
        /// <typeparam name="T">The type of the singleton</typeparam>
        /// <param name="instance">The instance of the singleton. Used for type inference</param>
        public static void Remove<T>(T instance) where T : Component
        {
            if (!HasInstance<T>())
                return;

            singletonInstances.Remove(typeof(T));
        }

        /// <summary>
        /// Clears all registered singletons<br></br>
        /// Useful when loading new scenes containing new instances of the same singletons
        /// </summary>
        /// <param name="destroyAll">Whether to destroy all instances of the registered singletons</param>
        public static void Clear(bool destroyAll = false)
        {
            Debug.Log(singletonInstances.Count);
            if(destroyAll)
            {
                foreach (Component singleton in singletonInstances.Values)
                {
                    Object.Destroy(singleton);
                }
            }
            
            singletonInstances.Clear();
            
            Debug.Log(singletonInstances.Count);
        }
        
        static T SearchOrCreateInstance<T>(bool doException = true) where T : Component
        {
            Component instance = UnityEngine.Object.FindFirstObjectByType<T>();

            //Create a new instance of T if none is currently present
            if (instance == null)
            {
                if (!Application.isPlaying)
                {
                    if(doException)
                        throw new NullReferenceException($"No instance of '{typeof(T).Name}' was found. None will be created automatically as the singleton is being accessed in the editor");
                    
                    return null;
                }

                var obj = new GameObject($"{typeof(T).Name} Singleton");

                instance = obj.AddComponent<T>();

                //Creates the singleton
                Create(instance);
                StaticUtils.AutoLog($"No instance of '{typeof(T).Name}' has been found. One has been created automatically", LogSeverity.Warning, obj);
            }

            return (T)instance;
        }

        /// <summary>
        /// Checks whether the instance of a singleton exists
        /// </summary>
        /// <typeparam name="T">The type of the instance</typeparam>
        public static bool HasInstance<T>() where T : Component
        {
            return singletonInstances.ContainsKey(typeof(T));
        }
    }
}