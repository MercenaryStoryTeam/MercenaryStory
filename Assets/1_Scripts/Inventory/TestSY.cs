using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TestSY : MonoBehaviour
{
    private int currentOption;

    private void Update()
    {
        UIManager.Instance.inventory.TryOpenInventory();
    }

    private void ItemDropTest()
    {
        //ItemManager.Instance.AddItem();
        int randomDrop = Random.Range(0, 4);
        if (Input.GetKeyDown(KeyCode.O))
        {
            
        }
    }
}
