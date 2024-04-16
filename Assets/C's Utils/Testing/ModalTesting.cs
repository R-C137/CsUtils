using CsUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalTesting : MonoBehaviour
{
    [ContextMenu("Create Modal Window")]
    void CreateModal()
    {
        StaticUtils.CreateModalWindow("Test Question?", () => Debug.Log("Confirmed"), () => Debug.Log("Denied"), "Ya", "Nah");
    }
}
