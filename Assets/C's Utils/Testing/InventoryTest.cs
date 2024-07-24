using CsUtils.Systems.Inventory;
using System;
using System.Collections.Generic;
using UnityEngine;


public class InventoryTest : MonoBehaviour
{

    public Inventory<ItemBaseTest> inventory;

    public ItemBaseTest itemTOAdd;
    public int count;
    public bool prioNull;
    public bool exaactCompa;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        List<ItemBaseTest> list = new()
        {
            itemTOAdd,
            itemTOAdd,
            itemTOAdd
        };

        inventory = new(list, true);

        inventory.slots.Add(null);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("Add")]
    void Add()
    {
        Debug.Log(inventory.TryAdd(itemTOAdd, count, out _, prioNull));
    }
    [ContextMenu("Remove all")]
    void Remove()
    {
        inventory.Remove(itemTOAdd, exaactCompa);
    }

    [ContextMenu("Remove Count")]
    void RemoveCount()
    {
        inventory.Remove(itemTOAdd, count);
    }
}
