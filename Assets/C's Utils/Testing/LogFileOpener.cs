using CsUtils;
using CsUtils.Systems.Logging;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LogFileOpener : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //fs = File.Open(CsSettings.singleton.loggingFilePath, FileMode.Open, FileAccess.ReadWrite);
        int[] test = { 1, 2, 3, 5 };
        Logging.singleton.Log("Logging test passed. Testing with int data {0}", LogLevel.Info, forceStackTrace: true, parameters: test);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Logging.singleton.Log("Some error happened", LogLevel.Error);
        }
    }
}
