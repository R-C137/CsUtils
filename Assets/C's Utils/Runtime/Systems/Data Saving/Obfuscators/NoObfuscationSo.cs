/* NoObfuscationSo.cs - C's Utils
 *
 * An obfuscator which does not obfuscate data at all
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
using System.Text;
using UnityEngine;
namespace CsUtils.Systems.DataSaving
{
    [CreateAssetMenu(fileName = "No Obfuscation", menuName = "C's Utils/Data Saving/Obfuscation/No Obfuscation", order = 0)]
    public class NoObfuscationSo : DataObfuscatorSo
    {
        public override byte[] Obfuscate(string jsonData) => Encoding.UTF8.GetBytes(jsonData);

        public override string DeObfuscate(byte[] obfuscatedJsonData) => Encoding.UTF8.GetString(obfuscatedJsonData);
    }
}
