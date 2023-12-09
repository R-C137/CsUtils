using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CsUtils;
using System;
using CsUtils.Systems.Logging;

public class RandomWithWeight : MonoBehaviour
{
    public WeightedNumber[] weight;

    [ContextMenu("Get Random")]
    public void GetRandom()
    {
        Logging.singleton.Log("Random weight generated got {0}", LogSeverity.Info, writeToFile: false, parameters: StaticUtils.WeightedRandom(weight));
    }
}
