using CsUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DicTestEnum
{
    Val1,
    Val2,
    Val3,
    Val4,
    Val5,
    Val6
}

public class DictTest : MonoBehaviour
{
    public SerializableDictionary<SerializableDictionary<DicTestEnum, int>, string> testDict = new();
}
