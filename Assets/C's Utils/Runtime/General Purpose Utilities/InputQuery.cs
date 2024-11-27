/* InputQuery.cs - C's Utils
 * 
 * Offers utilities for Unity's (old) Input system
 * 
 * 
 * Creation Date: 04/08/2024
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [04/08/2024] - Initial implementation (C137)
 *      [27/11/2024] - Support for getting any key of a specific press type (C137)
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CsUtils
{
    public class InputQuery
    {
        public enum PressType
        {
            Pressed,
            Release,
            Held
        }
        
        public class KeyInfo
        {
            /// <summary>
            /// The actual key to be pressed
            /// </summary>
            public KeyCode key;

            /// <summary>
            /// The kind of press required for this key
            /// </summary>
            public PressType pressType;

            /// <summary>
            /// Whether this key will invalidate the sequence
            /// </summary>
            public bool invalidates;
            
            public KeyInfo(KeyCode key, PressType pressType, bool invalidates = false)
            {
                this.key = key;
                this.pressType = pressType;
                this.invalidates = invalidates;
            }
        }

        /// <summary>
        /// Reference to all the keys required to be pressed
        /// </summary>
        private readonly List<KeyInfo> requiredKeys = new();
        
        /// <summary>
        /// References to all the keys that any of them are required to be pressed
        /// </summary>
        private readonly List<KeyInfo> requiredAnyKeys = new();
        
        /// <summary>
        /// Returns true while the user holds down all the required keys
        /// </summary>
        public InputQuery WithKey(params KeyCode[] keys)
        {
            foreach (KeyCode key in keys)
            {
                requiredKeys.Add(new(key, PressType.Held));
            }
            return this;
        }

        /// <summary>
        /// Returns true the frame the user holds down all the required keys<br></br>
        /// May ot be suitable to have multiple keys in that context
        /// </summary>
        public InputQuery WithKeyDown(params KeyCode[] keys)
        {
            foreach (KeyCode key in keys)
            {
                requiredKeys.Add(new(key, PressType.Pressed));
            }
            return this;
        }

        /// <summary>
        /// Returns true the frame the user releases all the required keys<br></br>
        /// May ot be suitable to have multiple keys in that context
        /// </summary>
        public InputQuery WithKeyUp(params KeyCode[] keys)
        {
            foreach (KeyCode key in keys)
            {
                requiredKeys.Add(new(key, PressType.Release));
            }
            return this;
        }

        /// <summary>
        /// Returns true if any of these keys are pressed
        /// </summary>
        /// <param name="keys">The possible keys to press</param>
        public InputQuery WithAnyKey(params KeyCode[] keys)
        {
            foreach (KeyCode key in keys)
                requiredAnyKeys.Add(new KeyInfo(key, PressType.Held));
            return this;
        }
        
        /// <summary>
        /// Returns true if any of these keys are released by the user
        /// </summary>
        /// <param name="keys">The possible keys to release</param>
        public InputQuery WithAnyKeyDown(params KeyCode[] keys)
        {
            foreach (KeyCode key in keys)
                requiredAnyKeys.Add(new KeyInfo(key, PressType.Pressed));
            return this;
        }
        
        /// <summary>
        /// Returns true if any of these keys are released by the user
        /// </summary>
        /// <param name="keys">The possible keys to release</param>
        public InputQuery WithAnyKeyUp(params KeyCode[] keys)
        {
            foreach (KeyCode key in keys)
                requiredAnyKeys.Add(new KeyInfo(key, PressType.Release));
            return this;
        }
        
        /// <summary>
        /// Returns true while the user holds down all the required keys
        /// </summary>
        public static InputQuery GetKey(params KeyCode[] keys)
        {
            return new InputQuery().WithKey(keys);
        }
        
        /// <summary>
        /// Returns true the frame the user holds down all the required keys<br></br>
        /// May ot be suitable to have multiple keys in that context
        /// </summary>
        public static InputQuery GetKeyDown(params KeyCode[] keys)
        {
            return new InputQuery().WithKeyDown(keys);
        }
        
        /// <summary>
        /// Returns true the frame the user releases all the required keys<br></br>
        /// May not be suitable to have multiple keys in that context
        /// </summary>
        public static InputQuery GetKeyUp(params KeyCode[] keys)
        {
            return new InputQuery().WithKeyUp(keys);
        }

        /// <summary>
        /// Returns true if any of these keys are pressed
        /// </summary>
        /// <param name="keys">The possible keys to press</param>
        public static InputQuery GetAnyKeyDown(params KeyCode[] keys)
        {
            return new InputQuery().WithAnyKeyDown(keys);
        }
        
        /// <summary>
        /// Returns true if any of these keys are released by the user
        /// </summary>
        /// <param name="keys">The possible keys to release</param>
        public static InputQuery GetAnyKeyUp(params KeyCode[] keys)
        {
            return new InputQuery().WithAnyKeyUp(keys);
        }
        
        /// <summary>
        /// Adds a modifier to the sequence
        /// </summary>
        /// <param name="key">The modifier</param>
        /// <param name="pressType">What press type should that modifier be for</param>
        /// <returns></returns>
        public InputQuery WithModifier(KeyCode key, PressType pressType = PressType.Held)
        {
            requiredKeys.Add(new (key, pressType));
            return this;
        }

        /// <summary>
        /// Marks a key an invalidating the sequence for its press type
        /// </summary>
        /// <param name="key">The invalidator key</param>
        /// <param name="pressType">The press type of the invalidator key</param>
        public InputQuery Invalidate(KeyCode key, PressType pressType = PressType.Held)
        {
            requiredKeys.Add(new (key, pressType, true));
            return this;
        }

        //Ease of access
        public static bool anyKey => Input.anyKey;
        public static bool anyKeyDown => Input.anyKeyDown;

        /// <summary>
        /// Returns all the keys that are held down or pressed during that frame
        /// </summary>
        /// <returns></returns>
        public static KeyCode[] GetKeysHeld()
        {
            List<KeyCode> keys = new();
            if(!Input.anyKey)
                return keys.ToArray();
            
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if(Input.GetKeyDown(key) || Input.GetKey(key))
                    keys.Add(key);
            }

            return keys.ToArray();
        }

        /// <summary>
        /// Returns the key that was pressed during that frame
        /// </summary>
        /// <returns></returns>
        public static KeyCode GetKeyPressed()
        {
            if(!Input.anyKey)
                return KeyCode.None;
            
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if(Input.GetKeyDown(key))
                    return key;
            }

            return KeyCode.None;
        }
        
        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
                return false;
            if(ReferenceEquals(this, obj))
                return true;
            if(obj.GetType() != this.GetType())
                return false;
            return Equals((InputQuery)obj);
        }
        public override int GetHashCode()
        {
            return requiredKeys != null ? requiredKeys.GetHashCode() : 0;
        }
        
        public static bool operator ==(InputQuery query, bool value)
        {
            if(query == null)
                return false;
            
            foreach (KeyInfo key in query.requiredKeys)
            {
                bool pressed = IsKeyPressed(key);
                
                if (key.invalidates && pressed || !pressed && !key.invalidates)
                    return false;
            }

            return !query.requiredAnyKeys.Any() || query.requiredAnyKeys.Any(IsKeyPressed);
            
            bool IsKeyPressed(KeyInfo keyInfo)
            {
                switch (keyInfo.pressType)
                {
                    case PressType.Pressed:
                        return Input.GetKeyDown(keyInfo.key);
                    case PressType.Release:
                        return Input.GetKeyUp(keyInfo.key);
                    case PressType.Held:
                        return Input.GetKey(keyInfo.key);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public static bool operator !=(InputQuery query, bool value)
        {
            return !(query == value);
        }

        public static implicit operator bool(InputQuery query) => query == true;
    }
}
