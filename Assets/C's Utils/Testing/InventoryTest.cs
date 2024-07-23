using CsUtils.Systems.Inventory;
using System;
using UnityEngine;

[Serializable]
public class ItemBaseTest : IStackable
{
    public int id;
    public string name;

    public int stack;
    public int GetMaxStack()
    {
        return 25;
    }

    public int GetStack()
    {
        return stack;
    }

    public void SetStack(int stack)
    {
        this.stack = stack;
    }
}

public class InventoryTest : MonoBehaviour
{

    public Inventory<ItemBaseTest> inventory;

    public ItemBaseTest itemTOAdd;
    public int count;
    public bool prioNull;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("Add")]
    void Add()
    {
        inventory.TryAdd(itemTOAdd, count, out _, prioNull);
    }
}
