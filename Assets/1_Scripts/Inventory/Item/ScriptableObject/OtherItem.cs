using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Other Item", menuName = "Items/Other Item")]
[Serializable]
public class OtherItem : ItemBase
{
    public float sellPrice;
    public int stackAmount = 10;
    private void Awake()
    {
        isSellable = true;
        isStackable = true;
        itemClass = 2;
    }
}
