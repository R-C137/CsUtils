/* Base64ObfuscatorSo.cs - C's Utils
 *
 * An obfuscator which prevents data from being read using a normal text editor but can still be read with a hex editor.
 * It obfuscates data by converting it to base64. It is not intended to make editing save files impossible rather to make it harder to do.
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
 */
using System;
using System.Text;
using UnityEngine;
namespace CsUtils.Systems.DataSaving
{
    [CreateAssetMenu(fileName = "Base 64 Obfuscator", menuName = "C's Utils/Data Saving/Obfuscation/Base 64 Obfuscator", order = 1)]
    public class Base64ObfuscatorSo : DataObfuscatorSo
    {
        public override byte[] Obfuscate(string jsonData)
        {
            return Encoding.UTF8.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonData)));
        }
        public override string DeObfuscate(byte[] obfuscatedJsonData)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(obfuscatedJsonData)));
        }
        
    }
}
