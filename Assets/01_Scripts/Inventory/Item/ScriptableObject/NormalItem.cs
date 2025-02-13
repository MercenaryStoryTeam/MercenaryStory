using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Normal Item", menuName = "Items/Normal Item")]
[Serializable]
public class NormalItem : ItemBase
{
    private void Awake()
    {
        itemClass = 2;
    }
}
