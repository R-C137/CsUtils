using System;
using System.Collections.Generic;
using CsUtils;
using CsUtils.Systems.DataSaving;
using UnityEngine;

public class testing
{
    public string data = "none";
}
public class SoSavingTest : MonoBehaviour
{
    public PersistentProperty<List<List<testing>>> so;

    public ItemBaseTest item;
    
    public ItemBaseTest savedItem;
    
    private void Start()
    {
        so = new("testing.so.item", new());
    }

    private void Update()
    {
        if(InputQuery.GetKeyDown(KeyCode.K))
        {
            so.Value.Add(new List<testing>(){ new (){data = "testing data"}});
            so.UpdateToDisk();
        }
        if(InputQuery.GetKeyDown(KeyCode.L))
        {
        }
    }
}
