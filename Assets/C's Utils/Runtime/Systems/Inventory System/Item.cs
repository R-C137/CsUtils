/* Item.cs - C's Utils
 * 
 * A modular scriptable object, intended to work hand-in-hand with the inventory system
 * 
 * 
 * Creation Date: 29/11/2023
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [29/11/2023] - Initial implementation (C137)
 *      [09/12/2023] - Removed unnecessary using statements (C137)
 */
using UnityEngine;


namespace CsUtils.Systems.Inventory
{
    [CreateAssetMenu(fileName = "ItemSO", menuName = "C's Utils/Inventory/Item", order = 1)]
    public class Item : ScriptableObject
    {
        /// <summary>
        /// The name of the item that will be displayed to the user (where applicable)
        /// </summary>
        public string displayName;

        /// <summary>
        /// The maximum amount of items that can be stacked within this item
        /// </summary>
        public int maxStack;
    }
}
