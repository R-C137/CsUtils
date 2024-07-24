/* Inventory.cs - C's Utils
 * 
 * A scalable, general purpose, easy to use inventory system based on Minecraft's inventory
 * 
 * 
 * Creation Date: 29/11/2023
 * Authors: C137
 * Original: C137
 * 
 * Edited By: C137
 * 
 * Changes: 
 *      [30/11/2023] - Initial implementation (C137)
 *      [11/04/2023] - Added support for removing a finite quantity at an index (C137)
 *      [15/04/2024] - Made item generic (C137)
 *      [21/04/2024] - Fixed item addition not working in certain situations (C137)
 *                   - Added a function to search for the presence of an item within the inventory (C137)
 *      
 *      [01/05/2024] - Removed the need for generic items to inherit from 'IInventoryItem<T>' (C137)
 *                   - 'IEquatable<T>' & 'IStackable<T>' can be optionally inherited from the generic item (C137)
 *                   - Item null check is handled directly by the inventory (C137)
 *                   
 *      [23/07/2024] - Inventory can now be enumerated over (C137)
 *                   - Improved constructing inventory with items (C137)
 *                   - Changed name of variable 'items' to 'slots' (C137)
 *                   - Added a function to get the stack of an item (C137)
 *                   - Improved item queries (C137)
 *                   - Items can now track the slot they are in (C137)
 *                   - Improved item additions (C137)
 *      
 *      [24/07/2024] - Total inventory rework (C137)
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CsUtils.Extensions;
using UnityEngine;

namespace CsUtils.Systems.Inventory
{
    public interface ICloneable<T>
    {
        /// <summary>
        /// Returns a clone of T
        /// </summary>
        public T Clone();
    }

#pragma warning disable IDE1006 // Naming Styles
    public interface IItem<T> : ICloneable<T> where T : class
    {
        /// <summary>
        /// The amount of item stacked within the slot this item is found in
        /// </summary>
        /// <returns></returns>
        public int stack { get; set; }

        /// <summary>
        /// The maximum amount of times that this item can be stacked
        /// </summary>
        public int maxStack { get; set; }

        /// <summary>
        /// The slot that this item is found in
        /// </summary>
        /// <returns></returns>
        public int slot { get; set; }

        /// <summary>
        /// Whether unity will serialize this item<br></br>
        /// When unity serializes an item that is not a ScriptableObject, it will never be null. Setting this value to true will make the inventory use a work-around
        /// </summary>
        public bool isUnitySerializable { get; set; }

        /// <summary>
        /// Whether the item has a value (is not null)
        /// </summary>
        public bool isNull { get; set; }


        /// <summary>
        /// Checks whether the queried item is of the same kind as this item
        /// </summary>
        /// <param name="item">The item to query</param>
        /// <returns></returns>
        public bool Is(T item);
    }
#pragma warning restore IDE1006 // Naming Styles

    [Serializable]
    public class Inventory<T> : IEnumerable<T> where T : class, IItem<T>
    {
        //Delegates used to raise events
        #region Delegates
        /// <summary>
        /// Called when an item is added to the inventory
        /// </summary>
        /// <param name="item">The item that was added</param>
        /// <param name="stackAdded">The stack that was added</param>
        /// <param name="stackOnly">Whether only the stack was increased</param>
        public delegate void ItemAdded(T item, int stackAdded, bool stackOnly);
        /// <summary>
        /// Called when an item is removed from the inventory
        /// </summary>
        /// <param name="item">The item that was removed</param>
        /// <param name="stackRemoved">The stack that was removed</param>
        /// <param name="stackOnly">Whether only the stack was decreased</param>
        public delegate void ItemRemoved(T item, int stackRemoved, bool stackOnly);
        public delegate void ItemSwapped(T itemA, T itemB);
        public delegate void ItemReplaced(T repalcedItem, T previousItem, int indexReplaced);

        #endregion

        /// <summary>
        /// Contains references to all the slots in the inventory
        /// </summary>
        public List<T> slots;

        //Only one event is raised per operation
        //i.e neither 'onItemAdded' nor 'onItemRemoved' will be raised when 'InsertItem(InventoryItem)' is called

        #region Events
        /// <summary>
        /// Raised when a new item is added to the inventory
        /// </summary>
#pragma warning disable IDE1006 // Naming Styles
        public event ItemAdded onItemAdded;
        /// <summary>
        /// Raised when an item is removed from the inventory
        /// </summary>
        public event ItemRemoved onItemRemoved;
        /// <summary>
        /// Raised when two items swap places in the inventory
        /// </summary>
        public event ItemSwapped onItemSwapped;
        /// <summary>
        /// Raised when an item is inserted in the inventory
        /// </summary>
        public event ItemReplaced onItemInserted;
#pragma warning restore IDE1006 // Naming Styles
        #endregion

        //All kinds of constructors are available for simplicity
        #region Constructors

        /// <summary>
        /// Instantiates a new inventory with predefined items
        /// </summary>
        /// <param name="items">The items to add</param>
        /// <param name="cloneItems">Whether to clone the items before adding them</param>
        public Inventory(IEnumerable<T> items, bool cloneItems = false)
        {
            slots = new List<T>();

            for (int i = 0; i < items.Count(); i++)
            {
                T item = items.ElementAt(i);
                item.stack = 1;

                T finalItem = cloneItems ? item.Clone() : item;
                slots.Add(finalItem);
                finalItem.slot = i;
            }
        }

        /// <summary>
        /// Instantiates a new inventory with predefined amount of slots<br></br>
        /// </summary>
        /// <param name="slotCount">The maximum amount of slots the inventory has</param>
        public Inventory(int slotCount)
        {
            for (int i = 0; i < slotCount; i++)
            {
                slots.Add(null);
            }
        }
        #endregion

        //Handles all the queries of the inventory
        //Note: Needs to be tested
        #region Queries

        /// <summary>
        /// Used internally to check if an item is null
        /// </summary>
        /// <param name="item">The item to check</param>
        internal bool CheckNull(T item)
        {
            return item == null || item.isNull;
        }

        /// <summary>
        /// Simplify access to the array of items<br></br>
        /// Note: Changing the values of the array will cause the corresponding events to NOT be raised
        /// </summary>
        public T this[int index] => slots[index];

        /// <summary>
        /// Gets the enumerator for the items
        /// </summary>
        public IEnumerator<T> GetEnumerator() => slots.GetEnumerator();

        // Explicit implementation of non-generic IEnumerable
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Gets all the indexes of the slots that have space for a given item
        /// </summary>
        /// <param name="item">The item to check for</param>
        /// <param name="prioritiseNull">Whether null indexes should be at the top or the bottom of the array</param>
        /// <returns>Indexes of the available slots</returns>
        public int[] GetStackableIndexes(T item, bool prioritiseNull = false)
        {
            List<int> result = new();

            for (int i = 0; i < slots.Count; i++)
            {
                T itemToCheck = slots[i];

                if (CheckNull(itemToCheck) || (itemToCheck.Is(item) && itemToCheck.stack < itemToCheck.maxStack))
                    result.Add(i);
            }

            return result.OrderBy(a => prioritiseNull ^ CheckNull(slots[a])).ToArray();
        }

        /// <summary>
        /// Inserts an item at the given slot
        /// </summary>
        /// <param name="item">The item to be inserted</param>
        /// <param name="index">The index at which the item will be inserted</param>
        /// <param name="cloneItem">Whether the item to be inserted should be cloned</param>
        /// <returns>The item that was previously at the selected index</returns>
        public T Replace(T item, int index, bool cloneItem = true)
        {
            T finalItem = cloneItem ? item.Clone() : item;
            T oldItem = slots.Replace(index, finalItem);

            finalItem.slot = index;
            onItemInserted?.Invoke(finalItem, oldItem, index);

            return oldItem;
        }

        /// <summary>
        /// Swaps two indexes
        /// </summary>
        /// <param name="indexA">The first index to swap</param>
        /// <param name="indexB">The second index to swap</param>
        public void Swap(int indexA, int indexB)
        {
            (slots[indexA], slots[indexB]) = (slots[indexB], slots[indexA]);

            slots[indexA].slot = indexA;
            slots[indexB].slot = indexB;

            //Since the indexes have been swapped, we need to swap them again when raising the event
            onItemSwapped?.Invoke(slots[indexB], slots[indexA]);
        }
        #endregion

        #region Item Addition
        /// <summary>
        /// Tries to add an item at the first available index. Will clone the item if it cannot be stacked
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <param name="prioritiseNull">Whether null indexes should be prioritised when searching for available indexes</param>
        /// <returns>Whether adding the item was successful</returns>
        public bool TryAdd(T item, bool prioritiseNull = false)
        {
            int[] indexes = GetStackableIndexes(item, prioritiseNull);

            if (!indexes.Any())
                return false;

            int index = indexes.First();

            if (!CheckNull(slots[index]))
            {
                slots[index].stack++;

                onItemAdded?.Invoke(slots[index], 1, true);
            }
            else
            {
                slots[index] = item.Clone();
                slots[index].stack = 1;
                slots[index].slot = index;

                onItemAdded?.Invoke(slots[index], 1, false);
            }

            return true;
        }


        /// <summary>
        /// Adds a stack of items of the same type to the first available indexes. Will clone the item if it cannot be stacked
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <param name="stack">The stack of that item to add</param>
        /// <param name="amountLeft">The amount that couldn't be added</param>
        /// <param name="prioritiseNull">Whether null indexes should be prioritised when searching for available indexes</param>
        /// <returns>Whether all the items have been added</returns>
        public bool TryAdd(T item, int stack, out int amountLeft, bool prioritiseNull = false)
        {
            if (stack <= 0)
            {
                amountLeft = 0;
                return true; // Since there was nothing to add, we can consider it to be successfully added
            }

            //If only a single stack is being added, there's no need for complex calculations
            if (stack == 1)
            {
                bool couldAdd = TryAdd(item, prioritiseNull);
                amountLeft = couldAdd ? 0 : 1;

                return couldAdd;
            }

            int[] indexes = GetStackableIndexes(item, prioritiseNull);

            amountLeft = stack;

            if (!indexes.Any())
                return false;

            for (int i = 0; i < indexes.Length; i++)
            {
                if (!CheckNull(slots[indexes[i]]))
                {
                    int amountAdded = AddToStack(slots[indexes[i]], ref amountLeft);
                    onItemAdded?.Invoke(slots[indexes[i]], amountAdded, true);
                }
                else
                {
                    slots[indexes[i]] = item.Clone();
                    slots[indexes[i]].stack = 0;
                    slots[indexes[i]].slot = indexes[i];

                    int amountAdded = AddToStack(slots[indexes[i]], ref amountLeft);
                    onItemAdded?.Invoke(slots[indexes[i]], amountAdded, false);
                }

                //Check if all the stacks have been added
                if (amountLeft == 0)
                    return true;
            }
            return false;

            static int AddToStack(T itemToAdd, ref int amountToAdd)
            {
                int amountAddable = CalculateAmountAddable(itemToAdd.maxStack, itemToAdd.stack, amountToAdd);
                itemToAdd.stack += amountAddable;

                amountToAdd -= amountAddable;

                //Return the amount that was added
                return amountToAdd;
            }

            static int CalculateAmountAddable(int itemMaxStack, int itemCurrentStack, int stackToAdd) => Mathf.Clamp(stackToAdd, 0, itemMaxStack - itemCurrentStack);
        }
        #endregion

        #region Item Removal
        /// <summary>
        /// Removes the the select type of item from the inventory
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <param name="exactComparison">Whether to remove the reference to the item or its kind</param>
        /// <returns>Whether removal was successful.</returns>
        public bool Remove(T item, bool exactComparison = true)
        {
            if (!exactComparison)
            {
                var removales = slots.Where(t => t.Is(item));

                if (removales.Any())
                {
                    T toRemove = removales.First();

                    if(toRemove.isUnitySerializable)
                        toRemove.isNull = true;
                    else
                        slots[toRemove.slot] = null;

                    onItemRemoved?.Invoke(toRemove, toRemove.stack, false);
                    return true;
                }

                return false;
            }
            else if(slots.Any(t => t == item))
            {
                if(item.isUnitySerializable)
                    item.isNull = true;
                else
                    slots[item.slot] = null;

                onItemRemoved?.Invoke(item, item.stack, false);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes an item at an index
        /// </summary>
        /// <param name="amount">The amount of the item to remove</param>
        /// <param name="index">The index at which to remove the item</param>
        public void RemoveAt(int amount, int index)
        {
            int amountToRemove = Mathf.Min(slots[index].stack, amount);

            if ((slots[index].stack -= amountToRemove) == 0)
            {
                onItemRemoved?.Invoke(slots[index], amountToRemove, false);

                if (slots[index].isUnitySerializable)
                    slots[index].isNull = true;
                else
                    slots[index] = null;
            }
            else
                onItemRemoved?.Invoke(slots[index], amountToRemove, true);
        }

        /// <summary>
        /// Removes a stack of items of the same kind
        /// </summary>
        /// <param name="item">The kind of item to remove</param>
        /// <param name="stack">The stack to remove</param>
        public void Remove(T item, int stack)
        {
            if (stack <= 0)
                return;

            //If only a single stack is being removed, there's no need for complex calculations
            if (stack == 1)
            {
                Remove(item, false);
                return;
            }

            var items = slots.Where(t => !CheckNull(t) && t.Is(item) ).ToList();

            foreach (var t in items)
            {
                RemoveFromStack(t, ref stack);
            }

            int RemoveFromStack(T affectedItem, ref int amountToRemove)
            {
                int amountRemovable = Mathf.Clamp(stack, 0, affectedItem.stack);
                affectedItem.stack -= amountRemovable;

                amountToRemove -= amountRemovable;

                //Reset index if all the stacks were removed
                if (affectedItem.stack == 0)
                {
                    //Raise the event before the removal so that the proper value is passed
                    onItemRemoved?.Invoke(affectedItem, amountRemovable, false);

                    if (affectedItem.isUnitySerializable)
                        affectedItem.isNull = true;
                    else
                        slots[affectedItem.slot] = null;
                }
                else
                    onItemRemoved?.Invoke(affectedItem, amountRemovable, true);


                //Return the amount that was added
                return amountToRemove;
            }
        }
        #endregion
    }
}
