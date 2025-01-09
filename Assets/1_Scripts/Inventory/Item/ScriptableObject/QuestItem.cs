using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest Item", menuName = "Items/Quest Item")]
[Serializable]
public class QuestItem : ItemBase
{
    private void Awake()
    {
        isSellable = false;
        itemClass = 3;
    }
}
