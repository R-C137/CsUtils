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
 *      
 */
using CsUtils.Systems.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CsUtils
{
    public static class Singleton
    {
        static Dictionary<Type, Component> instances = new();
        static HashSet<Type> instanceHash = new();

        /// <summary>
        /// Declares a class as a singleton. Should be done on Awake()<br></br>
        /// If another singleton of the same type has already been declared, 'instance' will be destroyed
        /// </summary>
        /// <typeparam name="T">The type of the singleton to declare</typeparam>
        /// <param name="instance">The instance of the singleton</param>
        /// <returns>Whether the singleton was successfully created</returns>
        public static bool Create<T>(T instance) where T : Component
        {
            if(!instanceHash.Add(typeof(T)))
            {
                UnityEngine.Object.Destroy(instance);
                StaticUtils.AutoLog($"Two or more instances of '{typeof(T).Name}' exist, a singleton should only have one instance. '{instance.gameObject.name}.{typeof(T).Name}' has been destroyed",
                                    LogSeverity.Warning, instance.gameObject);
                return false;
            }    

            instances.Add(typeof(T), instance);

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

            return (T)instances[typeof(T)];
        }

#pragma warning disable IDE0060 // Remove unused parameter
        /// <summary>
        /// Removes a singleton from the declared list of singletons
        /// </summary>
        /// <typeparam name="T">The type of the singleton</typeparam>
        /// <param name="instance">The instance of the singleton. Used for type inference</param>
        public static void Remove<T>(T instance) where T : Component
        {
            if (!HasInstance<T>())
                return;

            instances.Remove(typeof(T));
            instanceHash.Remove(typeof(T));
        } 
#pragma warning restore IDE0060 // Remove unused parameter

        static T SearchOrCreateInstance<T>() where T : Component
        {
            Component instance = UnityEngine.Object.FindFirstObjectByType<T>();

            //Create a new instance of T if none is currently present
            if (instance == null)
            {
                if (!Application.isPlaying)
                {
                    throw new NullReferenceException($"No instance of '{typeof(T).Name}' was found. None will be created automatically as the singleton isn't being accessed at runtime");
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
            return instanceHash.Contains(typeof(T));
        }
    }
}