using CsUtils.Systems.DataSaving;
using CsUtils.Systems.Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeristentPropTest : MonoBehaviour
{
    public PersistentProperty<float> testProp;

    private void Awake()
    {
        testProp = new("test.testPropL");
    }

    [ContextMenu("Set Value p5")]
    void UpdateValue()
    {
        testProp.Value = .126232083f;
        Logging.Log("Value of prop is now {0}", LogSeverity.Info, parameters: testProp.Value);
    }

    [ContextMenu("Set Value 7")]
    void UpdateValueLol()
    {
        testProp.Value = 7;
        Logging.Log("Value of prop is now {0}", LogSeverity.Info, parameters: testProp.Value);
    }

    [ContextMenu("Get Value")]
    void GetValue()
    {
        Logging.Log("Value of prop is {0}", LogSeverity.Info, parameters: testProp.Value);
    }
}
