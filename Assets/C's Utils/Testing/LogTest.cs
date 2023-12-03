using CsUtils.Systems.Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogTest : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        int[] test = { 1, 2, 3, 5};
        Logging.singleton.Log("Logging test passed. Testing with int data {0}", LogLevel.Info, parameters: test);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
