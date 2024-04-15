using UnityEngine;
using CsUtils.Systems.Inventory;
using System;

[Serializable]
public struct TestItem : IInventoryItem<TestItem>
{
    //There should be some way of identifying the item
    public int id;

    //In practice, items would have some kind of data inside of them
    public object data;


    public readonly bool Equals(TestItem other)
    {
        //If the id is the same and the data is the same, we can be sure that the item 
        return other.id == id && other.data == data;
    }

    public readonly int GetMaxStack()
    {
        return 25;
    }

    public TestItem GetNull()
    {
        return new TestItem()
        {
            id = -1,
            data = null
        };
    }

}

public class InventoryTest : MonoBehaviour
{
    public Inventory<TestItem> inventory;

    public TestItem itemToAdd;

    public int index;

    public int amount = 1;

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log(inventory.items[0].Equals(null));

        //Debug.Log(inventory.GetIndexes(indexedItem)?.Length);

        //var tmp = inventory.GetEmptyIndexes(null, false);
        //Debug.Log(string.Join(',', tmp));

    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    inventory.RemoveAt(amount, index);
        //}

        //if (Input.GetKeyDown(KeyCode.V))
        //    Debug.Log(inventory.TryAdd(itemToAdd));

        //if (Input.GetKeyDown(KeyCode.B))
        //    inventory.Remove(itemToAdd, amount);
    }
}
