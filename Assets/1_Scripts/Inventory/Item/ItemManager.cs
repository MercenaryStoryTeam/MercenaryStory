using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using System.IO;
using File = System.IO.File;

public class ItemManager : SingletonManager<ItemManager>
{
    [SerializeField] public string jsonFilePath = "Assets/9_Items/items.json";
    private ItemList itemList;
    private void Start()
    {
        LoadItemsFromJson();
    }

    private void LoadItemsFromJson()
    {
    }

    public Item GetItemById(int id)
    {
        foreach (var item in itemList.items)
        {
            if (item.item_Id == id)
            {
                return item;
            }
        }
        Debug.Log($"ID에 해당하는 아이템이 없습니다.");
        return null;
    }
}
