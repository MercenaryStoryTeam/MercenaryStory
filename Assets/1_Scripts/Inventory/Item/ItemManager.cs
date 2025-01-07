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
        // string fullPath = Path.Combine(Application.dataPath, jsonFilePath);
        // if (File.Exists(fullPath))
        // {
        //     string jsonData = File.ReadAllText(fullPath);
        //     itemList = JsonUtility.FromJson<ItemList>($"{{\"items\":{jsonData}}}");
        //     
        //     foreach (var item in itemList.items)
        //     {
        //         Debug.Log($"아이템 ID: {item.item_Id}\n아이템 이름: {item.item_Name}\n아이템 가격:{item.item_Cellprice}");
        //     }
        // }
        //
        // else
        // {
        //     Debug.Log("json 파일 찾을 수 없음");
        // }
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
