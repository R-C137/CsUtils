using CsUtils.Systems.Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataSavingTest : MonoBehaviour
{
    public string id;

    public string value;

    // Start is called before the first frame update
    void Start()
    {
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Logging.Log("Loaded data with id: {0}, got value {1}", LogSeverity.Info, parameters: new object[] { id, GameData.Get(id, value) });
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            Logging.Log("Saved data with id: {0}, and value {1}", LogSeverity.Info, parameters: new object[] { id, GameData.Set(id, value) });
        }
    }
}
