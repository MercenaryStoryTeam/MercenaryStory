using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<ItemBase> items;
    public GameObject slot;
    
    private void Update()
    {
    }

    private void AddItemToInventory(ItemBase item)
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            items.Add(item);
            
        }
    }
}
