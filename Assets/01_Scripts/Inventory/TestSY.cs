using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TestSY : MonoBehaviour
{
    private Inventory inventory;
    public ItemBase currentWeapon;
    public float myGold = 0;
    
    private void Awake()
    {
        inventory = FindObjectOfType<Inventory>();
    }

    private void Update()
    {
        UIManager.Instance.inventory.TryOpenInventory();
        UIManager.Instance.shop.TryOpenShop();
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ItemBase randomItem = inventory.RandomDropItems();
            if (randomItem != null)
            {
                inventory.AddItemToInventory(randomItem);
            }
        }
    }
    
}
