using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TestSY : MonoBehaviour
{
    public ItemBase currentWeapon;
    public float myGold = 0;
    
    private PlayerTestSY playerTest;
    private void Awake()
    {
        playerTest = FindObjectOfType<PlayerTestSY>();
    }

    private void Update()
    {
        UIManager.Instance.inventory.TryOpenInventory();
        UIManager.Instance.shop.TryOpenShop();
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
                playerTest.TestDrop(InventoryManger.Instance.RandomDropItems());
        }
    }
    
}
