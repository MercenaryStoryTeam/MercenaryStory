using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour
{
    public Button itemSlot;
    private List<ItemBase> myItems;
    private void Awake()
    {
        itemSlot.onClick.AddListener(ItemSlotClick);
        myItems = ItemManager.Instance.items;
    }

    private void ItemSlotClick()
    {
        throw new NotImplementedException();
    }
}
