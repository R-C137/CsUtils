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
 */
using System;
using System.Collections.Generic;
using System.Linq;
using CsUtils.Extensions;
using UnityEngine;

namespace CsUtils.Systems.Inventory
{
    //Delegates used to raise events
    #region Delegates
    /// <summary>
    /// Called when an item is added to the inventory
    /// </summary>
    /// <param name="item">The item that was added</param>
    /// <param name="stackAdded">The stack that was added</param>
    /// <param name="stackOnly">Whether only the stack was increased</param>
    public delegate void ItemAdded(InventoryItem item, int stackAdded, bool stackOnly);
    /// <summary>
    /// Called when an item is removed from the inventory
    /// </summary>
    /// <param name="item">The item that was removed</param>
    /// <param name="stackRemoved">The stack that was removed</param>
    /// <param name="stackOnly">Whether only the stack was decreased</param>
    public delegate void ItemRemoved(InventoryItem item, int stackRemoved, bool stackOnly);
    public delegate void ItemSwapped(InventoryItem itemA, InventoryItem itemB);
    public delegate void ItemReplaced(InventoryItem repalcedItem, InventoryItem previousItem, int indexReplaced);
    #endregion

    [Serializable]
    public struct InventoryItem
    {
        /// <summary>
        /// Reference to the actual scriptable object of this item
        /// </summary>
        public Item itemSO;

        /// <summary>
        /// The amount of item stacked within this item
        /// </summary>
        public int stack;

        //Allows the struct to be 'nullable' in a way. Null check against an inventory item will always be done against the scriptable object, this override saves time
        public override readonly bool Equals(object obj)
        {
            if(obj == null && itemSO == null)
                return true;

            return base.Equals(obj);
        }

        //Allows the user to do the normal equals check, which might be needed in some cases
        public readonly bool NormalEquals(object obj)
        {
            return base.Equals(obj);
        }

        public override readonly int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [Serializable]
    public class Inventory
    {
        /// <summary>
        /// Contains references to all the items in the inventory
        /// </summary>
        public InventoryItem[] items;

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
        /// <param name="items">The array of inventory items to add</param>
        public Inventory(InventoryItem[] items) => this.items = items;

        /// <summary>
        /// Instantiates a new inventory with predefined items
        /// </summary>
        /// <param name="items">The list of inventory items to add</param>
        public Inventory(List<InventoryItem> items) : this(items.ToArray()) { }

        /// <summary>
        /// Instantiates a new inventory with predefined items
        /// </summary>
        /// <param name="items">The array of items to add</param>
        public Inventory(Item[] items)
        {
            this.items = new InventoryItem[items.Length];

            for (int i = 0; i < items.Length; i++)
            {
                this.items[i].itemSO = items[i];
            }
        }

        /// <summary>
        /// Instantiates a new inventory with predefined items
        /// </summary>
        /// <param name="items">The list of items to add</param>
        public Inventory(List<Item> items) : this(items.ToArray()) { }

        /// <summary>
        /// Instantiates a new inventory with predefined amount of slots
        /// </summary>
        /// <param name="slotCount">The maximum amount of slots the inventory has</param>
        public Inventory(int slotCount) => items = new InventoryItem[slotCount];
        #endregion

        /// <summary>
        /// Simplify access to the array of items<br></br>
        /// Note: Changing the values of the array will cause the corresponding events to NOT be raised
        /// </summary>
        public InventoryItem this[int index] => items[index];

        //Simplify the search for the slots of items in the inventory
        //Note: Needs to be tested
        #region Index Search
        public int? FirstIndexOf(Item item) => GetIndexes(item).FirstOrDefault();

        public int? LastIndexOf(Item item) => GetIndexes(item).LastOrDefault();

        public int[] GetIndexes(Item item) => StaticUtils.GetIndexesOf(items, items.Where(a => a.itemSO == item).ToArray());

        /// <summary>
        /// Gets the first index of the occurrences of an item
        /// </summary>
        /// <param name="item">The item to query</param>
        /// <param name="normalEqualityCheck">Whether to use the non-overriden 'Equals()' method</param>
        /// <returns>The first index of the occurences of the queried item</returns>
        public int? FirstIndexOf(InventoryItem item, bool normalEqualityCheck = false) => GetIndexes(item, normalEqualityCheck).FirstOrDefault();

        /// <summary>
        /// Gets the last index of the occurrences of an item
        /// </summary>
        /// <param name="item">The item to query</param>
        /// <param name="normalEqualityCheck">Whether to use the non-overriden 'Equals()' method</param>
        /// <returns>The last index of the occurences of the queried item</returns>
        public int? LastIndexOf(InventoryItem item, bool normalEqualityCheck = false) => GetIndexes(item, normalEqualityCheck).LastOrDefault();

        /// <summary>
        /// Gets all indexes of the occurrences of an item
        /// </summary>
        /// <param name="item">The item to query</param>
        /// <param name="normalEqualityCheck">Whether to use the non-overriden 'Equals()' method</param>
        /// <returns>Indexes of the occurences of the queried item</returns>
        public int[] GetIndexes(InventoryItem item, bool normalEqualityCheck = false) => StaticUtils.GetIndexesOf(items, items.Where(a => normalEqualityCheck ? a.NormalEquals(item) : a.Equals(item)).ToArray());


        /// <summary>
        /// Gets all the indexes of the slots that have space for a given item
        /// </summary>
        /// <param name="item">The item to check for</param>
        /// <param name="prioritiseNull">Whether null indexes should be at the top or the bottom of the array</param>
        /// <returns>Indexes of the available slots</returns>
        public int[] GetEmptyIndexes(Item item, bool prioritiseNull = true)
        {
            List<int> result = new();

            for (int i = 0; i < items.Length; i++)
            {
                InventoryItem itemToCheck = items[i];

                if (itemToCheck.Equals(null) || (itemToCheck.itemSO.Equals(item) && itemToCheck.stack < itemToCheck.itemSO.maxStack))
                    result.Add(i);
            }

            return result.OrderBy(a => prioritiseNull ^ items[a].Equals(null)).ToArray();
        }

        /// <summary>
        /// Gets all the indexes of the slots that have space for a given item
        /// </summary>
        /// <param name="item">The item to check for</param>
        /// <param name="prioritiseNull">Whether null indexes should be at the top or the bottom of the array</param>
        /// <returns>Indexes of the available slots</returns>
        public int[] GetEmptyIndexes(InventoryItem item, bool prioritiseNull = true) => GetEmptyIndexes(item.itemSO, prioritiseNull);
        #endregion

        /// <summary>
        /// Inserts an item at the given slot
        /// </summary>
        /// <param name="item">The item to be inserted</param>
        /// <param name="index">The index at which the item will be inserted</param>
        /// <returns>The item that was previously at the selected index</returns>
        public InventoryItem Replace(InventoryItem item, int index)
        {
            InventoryItem oldItem = items.Replace(index, item);

            onItemInserted?.Invoke(item, oldItem, index);

            return oldItem;
        }

        /// <summary>
        /// Swaps two indexes
        /// </summary>
        /// <param name="indexA">The first index to swap</param>
        /// <param name="indexB">The second index to swap</param>
        public void Swap(int indexA, int indexB)
        {
            (items[indexA], items[indexB]) = (items[indexB], items[indexA]);

            //Since the indexes have been swapped, we need to swap them again when raising the event
            onItemSwapped?.Invoke(items[indexB], items[indexA]);
        }

        #region Item Addition
        /// <summary>
        /// Tries to add an item at the first available index
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <param name="prioritiseNull">Whether null indexes should be prioritised when searching for available indexes</param>
        /// <returns>Whether adding the item was successful</returns>
        public bool TryAdd(Item item, bool prioritiseNull = true)
        {
            int[] indexes = GetEmptyIndexes(item, prioritiseNull);

            if (!indexes.Any())
                return false;

            int index = indexes.First();

            if (items[index].itemSO != null)
            {
                items[index].stack++;

                onItemAdded?.Invoke(items[index], 1, true);
            }
            else
            {
                items[index].itemSO = item;
                items[index].stack = 1;

                onItemAdded?.Invoke(items[index], 1, false);
            }

            return true;
        }

        /// <summary>
        /// Tries to add an item at the first available index
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <param name="prioritiseNull">Whether null indexes should be prioritised when searching for available indexes</param>
        /// <returns>Whether adding the item was successful</returns>
        public bool TryAdd(InventoryItem item, bool prioritiseNull = true) => TryAdd(item.itemSO, prioritiseNull);

        /// <summary>
        /// Adds a stack of items of the same type to the first available indexes
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <param name="stack">The stack of that item to add</param>
        /// <param name="amountLeft">The amount that couldn't be added</param>
        /// <param name="prioritiseNull">Whether null indexes should be prioritised when searching for available indexes</param>
        /// <returns>Whether all the items have been added</returns>
        public bool TryAdd(Item item, int stack, out int amountLeft, bool prioritiseNull = true)
        {
            //If only a single stack is being added, there's no need for complex calculations
            if (stack == 1)
            {
                bool couldAdd = TryAdd(item, prioritiseNull);
                amountLeft = couldAdd ? 0 : 1;

                return couldAdd;
            }

            int[] indexes = GetEmptyIndexes(item, prioritiseNull);

            amountLeft = stack;

            if (!indexes.Any())
                return false;

            for (int i = 0; i < indexes.Length; i++)
            {
                if (items[indexes[i]].itemSO != null)
                {
                    onItemAdded?.Invoke(items[indexes[i]], AddToStack(ref items[indexes[i]], ref amountLeft), true);
                }
                else
                {
                    items[indexes[i]].itemSO = item;
                    items[indexes[i]].stack = 0;

                    onItemAdded?.Invoke(items[indexes[i]], AddToStack(ref items[indexes[i]], ref amountLeft), false);
                }

                //Check if all the stacks have been added
                if (amountLeft == 0)
                    return true;
            }
            return false;

            static int AddToStack(ref InventoryItem itemToAdd, ref int amountToAdd)
            {
                int amountAddable = CalculateAmountAddable(itemToAdd.itemSO.maxStack, itemToAdd.stack, amountToAdd);
                itemToAdd.stack += amountAddable;

                amountToAdd -= amountAddable;

                //Return the amount that was added
                return amountToAdd;
            }

            static int CalculateAmountAddable(int itemMaxStack, int itemCurrentStack, int stackToAdd) => Mathf.Clamp(stackToAdd, 0, itemMaxStack - itemCurrentStack);
        }

        /// <summary>
        /// Adds a number of items of the same type to the first available indexes
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <param name="amount">The amount of that item to add</param>
        /// <param name="amountLeft">The amount that couldn't be added</param>
        /// <param name="prioritiseNull">Whether null indexes should be prioritised when searching for available indexes</param>
        /// <returns>Whether all the items have been added</returns>
        public bool TryAdd(InventoryItem item, int amount, out int amountLeft, bool prioritiseNull = true) => TryAdd(item.itemSO, amount, out amountLeft, prioritiseNull);
        #endregion

        #region Item Removal
        /// <summary>
        /// Removes the the select type of item from the inventory
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <param name="removeAll">Whether to remove all occurences of that item</param>
        /// <param name="normalEqualityCheck">Whether to use the non-overriden 'Equals()' method</param>
        public void Remove(InventoryItem item, bool removeAll = false, bool normalEqualityCheck = true)
        {
            int[] indexes = GetIndexes(item, normalEqualityCheck);

            if (indexes == null)
                return;

            RemoveFromIndexes(indexes, removeAll);
        }


        /// <summary>
        /// Removes the the select type of item from the inventory
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <param name="removeAll">Whether to remove all occurences of that item</param>
        public void Remove(Item item, bool removeAll = false)
        {
            int[] indexes = GetIndexes(item);

            if (indexes == null) return;

            RemoveFromIndexes(indexes, removeAll);
        }
        
        /// <summary>
        /// Handles the removal of items from a given array of indexes. Used to avoid code repetition
        /// </summary>
        /// <param name="indexes">The indexes at which to remove the items</param>
        /// <param name="removeAll">Whether to remove all the items</param>
        protected void RemoveFromIndexes(int[] indexes, bool removeAll = false)
        {
            for (int i = 0; i < (removeAll ? indexes.Length : 1); i++)
            {
                //Raise the event before the removal so that the proper value is passed
                onItemRemoved?.Invoke(items[indexes[i]], items[indexes[i]].stack, false);

                //Reset the index
                items[indexes[i]] = new();
            }
        }

        /// <summary>
        /// Removes an item at an index
        /// </summary>
        /// <param name="amount">The amount of the item to remove</param>
        /// <param name="index">The index at which to remove the item</param>
        public void RemoveAt(int amount, int index)
        {
            int amountToRemove = Mathf.Min(items[index].stack, amount);

            if ((items[index].stack -= amountToRemove) == 0)
            {
                onItemRemoved?.Invoke(items[index], amountToRemove, false);
                items[index] = new();
            }
            else
                onItemRemoved?.Invoke(items[index], amountToRemove, true);
        }

        /// <summary>
        /// Removes a stack of items of the same type
        /// </summary>
        /// <param name="item">The type of item to remove</param>
        /// <param name="stack">The stack to remove</param>
        public void Remove(Item item, int stack)
        {
            if(stack <= 0)
                return;

            //If only a single stack is being removed, there's no need for complex calculations
            if (stack == 1)
            {
                Remove(item, false);
                return;
            }

            int[] indexes = GetIndexes(item);

            if (indexes == null)
                return;

            for(int i = 0; i < indexes.Length; i++)
            {
                int currentIndex = indexes[i];

                RemoveFromStack(currentIndex, ref stack);

                //Reset index if all the stacks were removed
                if (items[currentIndex].stack == 0)
                    items[currentIndex] = new();

                if (stack == 0)
                    return;
            }

            int RemoveFromStack(int affectedIndex, ref int amountToRemove)
            {
                int amountRemovable = Mathf.Clamp(stack, 0, items[affectedIndex].stack);
                items[affectedIndex].stack -= amountRemovable;

                amountToRemove -= amountRemovable;

                //Reset index if all the stacks were removed
                if (items[affectedIndex].stack == 0)
                {
                    //Raise the event before the removal so that the proper value is passed
                    onItemRemoved?.Invoke(items[affectedIndex], amountRemovable, false);

                    items[affectedIndex] = new();
                }
                else
                    onItemRemoved?.Invoke(items[affectedIndex], amountRemovable, true);


                //Return the amount that was added
                return amountToRemove;
            }
        }
        #endregion
    }
}
