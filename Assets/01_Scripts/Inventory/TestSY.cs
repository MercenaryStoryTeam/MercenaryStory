using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class TestSY : MonoBehaviour
{
    private int currentOption;
    private Inventory inventory;
    public bool isEquipped = false;
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
