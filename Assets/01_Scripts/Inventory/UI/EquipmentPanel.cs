using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentPanel : MonoBehaviour
{
    
    public Image currentEquipImage;
    private TestSY _testSY;
    private void Awake()
    {
        _testSY = FindObjectOfType<TestSY>();
    }

    private void Update()
    {
       UpdateUI();
    }

    public void SetEquipImage(InventorySlot slot)
    {
        if (slot.item.itemClass == 1 && slot != null && slot.item != null)
        {
            if (_testSY.currentWeapon == null)
            {
                currentEquipImage.sprite = slot.item.image;
                _testSY.currentWeapon = slot.item;

                slot.RemoveItem();
                Debug.Log($"현재 장착한 장비 아이템: {_testSY.currentWeapon.name}");
            }
            else
            {
                ItemBase beforeWeapon = _testSY.currentWeapon;

                _testSY.currentWeapon = slot.item;
                currentEquipImage.sprite = _testSY.currentWeapon.image;
                slot.RemoveItem();
                
                slot.AddItem(beforeWeapon);
                
                Debug.Log($"현재 장착한 장비 아이템: {_testSY.currentWeapon.name}");
            }
        }
    }
    

    private void UpdateUI()
    {
        if (currentEquipImage.sprite == null)
        {
            currentEquipImage.enabled = false;
        }
        
        else
        {
            currentEquipImage.enabled = true;
        }
    }
}
