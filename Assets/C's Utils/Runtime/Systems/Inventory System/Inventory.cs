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
 */
using System;
using System.Collections.Generic;
using System.Linq;
using CsUtils.Extensions;
using UnityEngine;

namespace CsUtils.Systems.Inventory
{
    /// <summary>
    /// A way for items to have a stacking system
    /// </summary>
    public interface IStackable
    {
        /// <summary>
        /// Gets the maximum amount of this items that can be stacked in a slot
        /// </summary>
        public int GetMaxStack();
    }


    public interface IInventoryItem<T> : IEquatable<T>
    {
        ///// <summary>
        ///// Returns whether this item is null<br></br>
        ///// Used to check whether a slot is empty
        ///// </summary>
        //public bool IsNull();

        /// <summary>
        /// Returns a null version of this item<br></br>
        /// Sets the item's slot item value to this to mark it as null
        /// </summary>
        public T GetNull();

    }

    [Serializable]
    public class ItemSlot<T> where T : IInventoryItem<T>
    {
        /// <summary>
        /// Reference to the actual item of this slot
        /// </summary>
        public T item;

        /// <summary>
        /// The amount of item stacked within this slot
        /// </summary>
        public int stack;

#pragma warning disable IDE1006 // Naming Styles
        /// <summary>
        /// The maximum amount of items that can be stacked in this slot
        /// </summary>
        public int maxStackSize => item is IStackable ? (item as IStackable).GetMaxStack() : int.MaxValue;
#pragma warning restore IDE1006 // Naming Styles

        public ItemSlot<T> GetNull()
        {
            return new ItemSlot<T>()
            {
                item = item.GetNull()
            };
        }

        public override bool Equals(object obj)
        {
            if(obj == null && item.Equals(item.GetNull()))
                return true;

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(item, stack, maxStackSize);
        }
    }

    [Serializable]
    public class Inventory<T> where T : IInventoryItem<T>
    {
        //Delegates used to raise events
        #region Delegates
        /// <summary>
        /// Called when an item is added to the inventory
        /// </summary>
        /// <param name="item">The item that was added</param>
        /// <param name="stackAdded">The stack that was added</param>
        /// <param name="stackOnly">Whether only the stack was increased</param>
        public delegate void ItemAdded(ItemSlot<T> item, int stackAdded, bool stackOnly);
        /// <summary>
        /// Called when an item is removed from the inventory
        /// </summary>
        /// <param name="item">The item that was removed</param>
        /// <param name="stackRemoved">The stack that was removed</param>
        /// <param name="stackOnly">Whether only the stack was decreased</param>
        public delegate void ItemRemoved(ItemSlot<T> item, int stackRemoved, bool stackOnly);
        public delegate void ItemSwapped(ItemSlot<T> itemA, ItemSlot<T> itemB);
        public delegate void ItemReplaced(ItemSlot<T> repalcedItem, ItemSlot<T> previousItem, int indexReplaced);

        #endregion

        /// <summary>
        /// Contains references to all the items in the inventory
        /// </summary>
        public ItemSlot<T>[] items;

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
        public Inventory(ItemSlot<T>[] items) => this.items = items;

        /// <summary>
        /// Instantiates a new inventory with predefined items
        /// </summary>
        /// <param name="items">The list of inventory items to add</param>
        public Inventory(List<ItemSlot<T>> items) : this(items.ToArray()) { }

        /// <summary>
        /// Instantiates a new inventory with predefined items
        /// </summary>
        /// <param name="items">The array of items to add</param>
        public Inventory(T[] items)
        {
            this.items = new ItemSlot<T>[items.Length];

            for (int i = 0; i < items.Length; i++)
            {
                this.items[i].item = items[i];
            }
        }

        /// <summary>
        /// Instantiates a new inventory with predefined items
        /// </summary>
        /// <param name="items">The list of items to add</param>
        public Inventory(List<T> items) : this(items.ToArray()) { }

        /// <summary>
        /// Instantiates a new inventory with predefined amount of slots
        /// </summary>
        /// <param name="slotCount">The maximum amount of slots the inventory has</param>
        public Inventory(int slotCount) => items = new ItemSlot<T>[slotCount];
        #endregion

        /// <summary>
        /// Simplify access to the array of items<br></br>
        /// Note: Changing the values of the array will cause the corresponding events to NOT be raised
        /// </summary>
        public ItemSlot<T> this[int index] => items[index];

        //Simplify the search for the slots of items in the inventory
        //Note: Needs to be tested
        #region Index Search
        public int? FirstIndexOf(T item) => GetIndexes(item).FirstOrDefault();

        public int? LastIndexOf(T item) => GetIndexes(item).LastOrDefault();

        public int[] GetIndexes(T item) => StaticUtils.GetIndexesOf(items, items.Where(a => a.item.Equals(item)).ToArray());

        /// <summary>
        /// Gets the first index of the occurrences of an item
        /// </summary>
        /// <param name="item">The item to query</param>
        /// <returns>The first index of the occurences of the queried item</returns>
        public int? FirstIndexOf(ItemSlot<T> item) => GetIndexes(item).FirstOrDefault();

        /// <summary>
        /// Gets the last index of the occurrences of an item
        /// </summary>
        /// <param name="item">The item to query</param>
        /// <returns>The last index of the occurences of the queried item</returns>
        public int? LastIndexOf(ItemSlot<T> item) => GetIndexes(item).LastOrDefault();

        /// <summary>
        /// Gets all indexes of the occurrences of an item
        /// </summary>
        /// <param name="item">The item to query</param>
        /// <returns>Indexes of the occurences of the queried item</returns>
        public int[] GetIndexes(ItemSlot<T> item) => StaticUtils.GetIndexesOf(items, items.Where(a => a == item).ToArray());

        /// <summary>
        /// Checks whether the inventory contains the queried item
        /// </summary>
        /// <param name="item">The item to query</param>
        public bool HasItem(T item) => FirstIndexOf(item).HasValue;

        /// <summary>
        /// Checks whether the inventory contains the queried item
        /// </summary>
        /// <param name="item">The item to query</param>
        public bool HasItem(ItemSlot<T> item) => FirstIndexOf(item).HasValue;

        /// <summary>
        /// Gets all the indexes of the slots that have space for a given item
        /// </summary>
        /// <param name="item">The item to check for</param>
        /// <param name="prioritiseNull">Whether null indexes should be at the top or the bottom of the array</param>
        /// <returns>Indexes of the available slots</returns>
        public int[] GetEmptyIndexes(T item, bool prioritiseNull = true)
        {
            List<int> result = new();

            for (int i = 0; i < items.Length; i++)
            {
                ItemSlot<T> itemToCheck = items[i];

                if (itemToCheck.Equals(null) || (itemToCheck.item.Equals(item) && itemToCheck.stack < itemToCheck.maxStackSize))
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
        public int[] GetEmptyIndexes(ItemSlot<T> item, bool prioritiseNull = true) => GetEmptyIndexes(item.item, prioritiseNull);
        #endregion

        /// <summary>
        /// Inserts an item at the given slot
        /// </summary>
        /// <param name="item">The item to be inserted</param>
        /// <param name="index">The index at which the item will be inserted</param>
        /// <returns>The item that was previously at the selected index</returns>
        public ItemSlot<T> Replace(ItemSlot<T> item, int index)
        {
            ItemSlot<T> oldItem = items.Replace(index, item);

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
        public bool TryAdd(T item, bool prioritiseNull = true)
        {
            int[] indexes = GetEmptyIndexes(item, prioritiseNull);

            if (!indexes.Any())
                return false;

            int index = indexes.First();

            if (!items[index].Equals(null))
            {
                items[index].stack++;

                onItemAdded?.Invoke(items[index], 1, true);
            }
            else
            {
                items[index].item = item;
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
        public bool TryAdd(ItemSlot<T> item, bool prioritiseNull = true) => TryAdd(item.item, prioritiseNull);

        /// <summary>
        /// Adds a stack of items of the same type to the first available indexes
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <param name="stack">The stack of that item to add</param>
        /// <param name="amountLeft">The amount that couldn't be added</param>
        /// <param name="prioritiseNull">Whether null indexes should be prioritised when searching for available indexes</param>
        /// <returns>Whether all the items have been added</returns>
        public bool TryAdd(T item, int stack, out int amountLeft, bool prioritiseNull = true)
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
                if (!items[indexes[i]].Equals(null))
                {
                    int amountAdded = AddToStack(ref items[indexes[i]], ref amountLeft);
                    onItemAdded?.Invoke(items[indexes[i]], amountAdded, true);
                }
                else
                {
                    items[indexes[i]].item = item;
                    items[indexes[i]].stack = 0;

                    int amountAdded = AddToStack(ref items[indexes[i]], ref amountLeft);
                    onItemAdded?.Invoke(items[indexes[i]], amountAdded, false);
                }

                //Check if all the stacks have been added
                if (amountLeft == 0)
                    return true;
            }
            return false;

            static int AddToStack(ref ItemSlot<T> itemToAdd, ref int amountToAdd)
            {
                int amountAddable = CalculateAmountAddable(itemToAdd.maxStackSize, itemToAdd.stack, amountToAdd);
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
        public bool TryAdd(ItemSlot<T> item, int amount, out int amountLeft, bool prioritiseNull = true) => TryAdd(item.item, amount, out amountLeft, prioritiseNull);
        #endregion

        #region Item Removal
        /// <summary>
        /// Removes the the select type of item from the inventory
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <param name="removeAll">Whether to remove all occurences of that item</param>
        public void Remove(ItemSlot<T> item, bool removeAll = false)
        {
            int[] indexes = GetIndexes(item);

            if (indexes == null)
                return;

            RemoveFromIndexes(indexes, removeAll);
        }


        /// <summary>
        /// Removes the the select type of item from the inventory
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <param name="removeAll">Whether to remove all occurences of that item</param>
        public void Remove(T item, bool removeAll = false)
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
                items[indexes[i]] = items[indexes[i]].GetNull();
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
                items[index] = items[index].GetNull();
            }
            else
                onItemRemoved?.Invoke(items[index], amountToRemove, true);
        }

        /// <summary>
        /// Removes a stack of items of the same type
        /// </summary>
        /// <param name="item">The type of item to remove</param>
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

            int[] indexes = GetIndexes(item);

            if (indexes == null)
                return;

            for (int i = 0; i < indexes.Length; i++)
            {
                int currentIndex = indexes[i];

                RemoveFromStack(currentIndex, ref stack);

                //Reset index if all the stacks were removed
                if (items[currentIndex].stack == 0)
                    items[currentIndex] = items[currentIndex].GetNull();

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

                    items[affectedIndex] = items[affectedIndex].GetNull();
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
