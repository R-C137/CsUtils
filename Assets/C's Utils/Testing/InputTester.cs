using System;
using CsUtils;
using UnityEngine;

public class InputTester : MonoBehaviour
{

    private void Update()
    {
        
        if(InputQuery.GetKey(KeyCode.Q).WithKey(KeyCode.L).WithModifier(KeyCode.LeftShift).Invalidate(KeyCode.W))
            Debug.Log("Pressed");
    }
}
