using CsUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedCallingTest : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            StaticUtils.DelayedCall(1.5f, () => Debug.Log("Sucessfully called"));
        }
    }
}
