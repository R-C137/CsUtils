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
        Logging.singleton.LogDirect("Random weight generated got {0}", LogSeverity.Info, fileLogging: false, parameters: StaticUtils.WeightedRandom(weight));
    }
}
