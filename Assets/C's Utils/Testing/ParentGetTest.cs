using CsUtils;
using CsUtils.Systems.Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentGetTest : MonoBehaviour
{
    public Transform child;

    [ContextMenu("Get Parents")]
    public void GetParents()
    {
        Logging.Log("Got parents {0}", LogSeverity.Info, parameters: StaticUtils.GetParents(child));
    }
}
