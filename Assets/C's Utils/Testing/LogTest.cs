using CsUtils;
using CsUtils.Systems.Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogTest : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        //int[] test = { 1, 2, 3, 5};
        //Logging.singleton.Log("Logging test passed. Testing with int data {0}", LogLevel.Info, parameters: test);

    }

    [ContextMenu("Debug Log")]
    public void LogDebug()
    {
        CsSettings.singleton.logger.LogDirect("Debug Log", LogSeverity.Debug);
    }

    [ContextMenu("Log Info")]
    public void LogInfo()
    {
        CsSettings.singleton.logger.LogDirect("Info Log", LogSeverity.Info);
    }

    [ContextMenu("Log Warning")]
    public void LogWarning()
    {
        CsSettings.singleton.logger.LogDirect("Warning Log", LogSeverity.Warning);
    }

    [ContextMenu("Log Error")]
    public void LogError()
    {
        CsSettings.singleton.logger.LogDirect("Error Log", LogSeverity.Error);
    }

    [ContextMenu("Log Fatal")]
    public void LogFatal()
    {
        CsSettings.singleton.logger.LogDirect("Fatal Log", LogSeverity.Fatal);
    }

    // Update is called once per frame
    [ContextMenu("Throw Error")]
    public void ThrowError()
    {
        throw new System.Exception();
    }
}
