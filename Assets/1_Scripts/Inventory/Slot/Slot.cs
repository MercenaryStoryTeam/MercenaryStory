using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public List<ItemBase> items;
    private Inventory inventory;
    public GameObject slot;
    public Image itemImagePrefab;

    private void Awake()
    {
        inventory = FindObjectOfType<Inventory>();
    }

    public void AddItemToSlot(ItemBase item)
    {
        items.Add(item);
    }
    
    
    
}
