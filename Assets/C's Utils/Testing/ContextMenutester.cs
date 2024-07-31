using System;
using CsUtils;
using UnityEngine;

public class ContextMenutester : MonoBehaviour
{
    void SpawnContext()
    {
        new ContextMenuBuilder()
            .WithOption(() => Debug.Log("Option 1 Selected"), "Option 1", Color.cyan)
            .WithOption(() => Debug.Log("Option 2 slected"), "Option 2")
            .WithOption(() => Debug.Log("Option 2 slected"), "Option 2")
            .WithOption(() => Debug.Log("Option 2 slected"), "Option 2")
            .WithOption(() => Debug.Log("Option 2 slected"), "Option 2")
            .WithOption(() => Debug.Log("Option 2 slected"), "Option 2")
            .WithOption(null, null)
            .Build();
    }
}
