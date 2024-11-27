using System;
using CsUtils;
using UnityEngine;

public class ContextMenutester : MonoBehaviour
{
    private void Update()
    {
        if(InputQuery.GetKeyDown(KeyCode.O))
            SpawnContext();
    }

    void SpawnContext()
    {
        new ContextMenuBuilder()
            .WithOption("Option 0", () => Debug.Log("Option 1 Selected"), Color.cyan)
            .WithOption("Options 1", () => Debug.Log("Option 2 selected"))
            .WithOption("Options 2", () => Debug.Log("Option 2 selected"))
            .WithOption("Options 3", () => Debug.Log("Option 2 selected"))
            .WithOption("Options 4", () => Debug.Log("Option 2 selected"))
            .WithOption("Options 5", () => Debug.Log("Option 2 selected"))
            .WithOption(null, null)
            .Build();
    }
}
