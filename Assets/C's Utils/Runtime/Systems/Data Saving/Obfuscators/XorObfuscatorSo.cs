/* XorObfuscatorSo.cs - C's Utils
 *
 * An obfuscator which prevents data from being read using a normal text editor but can still be read if an XOR gate is applied onto each byte.
 * It obfuscates data by applying an xor gate with a specific key. It is not intended to make editing save files impossible rather to make it harder to do.
 *
 *
 * Creation Date: 24/01/2024
 * Authors: C137
 * Original: C137
 *
 * Edited By: C137
 *
 * Changes:
 *      [24/11/2024] - Initial implementation (C137)
 */
using System.Text;
using UnityEngine;
namespace CsUtils.Systems.DataSaving
{
    [CreateAssetMenu(fileName = "XOR Obfuscator", menuName = "C's Utils/Data Saving/Obfuscation/XOR Obfuscator", order = 2)]
    public class XorObfuscatorSo : DataObfuscatorSo
    {
        private const byte XORKey = 0xAA;

        public override byte[] Obfuscate(string jsonData)
        {
            return XorObfuscate(Encoding.UTF8.GetBytes(jsonData));
        }
        public override string DeObfuscate(byte[] obfuscatedJsonData)
        {
            return Encoding.UTF8.GetString(XorObfuscate(obfuscatedJsonData));
        }
        
        static byte[] XorObfuscate(byte[] input)
        {
            byte[] result = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                result[i] = (byte)(input[i] ^ XORKey);
            }
            return result;
        }
    }
}
