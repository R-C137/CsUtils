/* DataObfuscatorSo.cs - C's Utils
 *
 * Base class for all drag & drop obfuscators.
 * Allows obfuscators to exist as scriptableobjects and thus be put in the data sections manually using the inspector
 *
 *
 * Creation Date: 23/01/2024
 * Authors: C137
 * Original: C137
 *
 * Edited By: C137
 *
 * Changes:
 *      [23/11/2024] - Initial implementation (C137)
 * 
 */
using UnityEngine;
namespace CsUtils.Systems.DataSaving
{
    public abstract class DataObfuscatorSo : ScriptableObject, IDataObfuscator
    {
        public abstract byte[] Obfuscate(string jsonData);
        public abstract string DeObfuscate(byte[] obfuscatedJsonData);
    }
}
