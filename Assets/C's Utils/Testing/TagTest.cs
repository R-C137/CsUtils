using CsUtils.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagTest : MonoBehaviour
{
    public GameObject obj;

    public string tagComparison;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log(obj.HasTag(tagComparison));
        }
        if(Input.GetKeyDown(KeyCode.O))
        {
            obj.AddTag(tagComparison, true);
        }
    }
}
