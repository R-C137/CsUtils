using System;
using CsUtils;
using CsUtils.Systems.DataSaving;
using UnityEngine;

public class SoSavingTest : MonoBehaviour
{
    public PersistentProperty<ItemBaseTest> so;

    public ItemBaseTest item;
    
    public ItemBaseTest savedItem;
    
    private void Start()
    {
        so = new("testing.so.item", savedItem);
    }

    private void Update()
    {
        if(InputQuery.GetKeyDown(KeyCode.K))
            so.Value = savedItem;
        if(InputQuery.GetKeyDown(KeyCode.L))
            savedItem = so.Value;
    }
}
