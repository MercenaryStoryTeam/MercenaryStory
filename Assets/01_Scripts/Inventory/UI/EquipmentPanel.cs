using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentPanel : MonoBehaviour
{
    
    public Image currentEquipImage;
    private void Awake()
    {
    }

    private void Update()
    {
       UpdateUI();
    }

    public void SetEquipImage(InventorySlot slot)
    {
        if (slot.item.itemClass == 1 && slot != null && slot.item != null)
        {
            currentEquipImage.sprite = slot.item.image;
        }
    }
    
    private void UpdateUI()
    {
        print(currentEquipImage);
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
