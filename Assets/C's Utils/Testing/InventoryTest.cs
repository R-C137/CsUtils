using UnityEngine;
using CsUtils.Systems.Inventory;
using System;

public class InventoryTest : MonoBehaviour
{
    public Inventory inventory;

    public Item indexedItem;

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
        if (Input.GetKeyDown(KeyCode.T))
        {
            inventory.Remove(indexedItem, amount);
        }
    }
}
