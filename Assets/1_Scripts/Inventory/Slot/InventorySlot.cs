using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public ItemBase item;
    public Image itemImage;
    public Button itemButton;
    
    private void Awake()
    {
        itemButton.onClick.AddListener(itemButtonClick);
    }

    private void itemButtonClick()
    {
        UIManager.Instance.OpenItemInfoPanel();
        UIManager.Instance.SetItemInfoScreen(item);
    }

    private void Update()
    {
        UpdateUI();
    }

    public void AddItem(ItemBase newItem)
    {
        item = newItem;
    }

    public void UpdateUI()
    {
        if (item != null)
        {
            itemImage.sprite = item.image;
            itemImage.enabled = true;
        }
        else
        {
            itemImage.enabled = false;
        }
    }
    
    
}
