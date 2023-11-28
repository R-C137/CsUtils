using CsUtils.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorExtensionTest : MonoBehaviour
{
    public Color32 color;

    // Start is called before the first frame update
    void Start()
    {
        color = new Color().WithAlpha(251).WithRed(135).WithGreen(150).WithBlue(127).ToColor32();
    }

}
