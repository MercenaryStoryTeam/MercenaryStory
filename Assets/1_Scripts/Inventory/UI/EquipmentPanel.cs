using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentPanel : MonoBehaviour
{
    public Image currentEquipImage;
    private List<ItemBase> myItems;

    private void Awake()
    {
        myItems = ItemManager.Instance.items;
    }

    private void Update()
    {
       // ItemManager.Instance.SetCurrentEquipImage();
    }
}
