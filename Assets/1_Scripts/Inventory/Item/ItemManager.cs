using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : SingletonManager<ItemManager>
{
    public List<Item> items = new List<Item>();

    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            AddItem(items[0]);
        }
    }

    public void AddItem(Item item)
    {
        items.Add(item);
    }
}
