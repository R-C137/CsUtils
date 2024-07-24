using CsUtils.Systems.Inventory;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "C's Utils/Testing/Item", fileName = "New Item")]
public class ItemBaseTest : ScriptableObject, IItem<ItemBaseTest>
{
    public int id;

    [field: SerializeField]
    public bool isNull { get; set; }

    [field: SerializeField]
    public int stack { get; set; }
    [field: SerializeField]
    public int slot { get; set; }
    public int maxStack { get; set; } = 25;
    [field: SerializeField]
    public bool isUnitySerializable { get; set; } = false;

    public ItemBaseTest Clone()
    {
        ItemBaseTest clone = CreateInstance<ItemBaseTest>();
        clone.id = id;
        clone.stack = stack;
        clone.slot = slot;
        clone.maxStack = maxStack;
        clone.isUnitySerializable = isUnitySerializable;

        return clone;

        //return new()
        //{
        //    id = id,
        //    isNull = isNull,
        //    stack = stack,
        //    slot = slot,
        //    maxStack = maxStack,
        //    isUnitySerializable = isUnitySerializable
        //};
    }

    public bool Is(ItemBaseTest item)
    {
        return item.id == id;
    }

    public int GetMaxStack()
    {
        return 25;
    }
}
