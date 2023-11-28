using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletontestHelper : MonoBehaviour
{
    private void Start()
    {
        Debug.Log(SingletonTest.singleton.gameObject.name);
    }
}
