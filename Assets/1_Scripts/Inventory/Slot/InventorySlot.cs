using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public ItemBase item;
    public Image itemImage;
    public Button itemButton;
    public Text itemCountText;
    
    private int slotCount = 0;

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
        if (item == null)
        {
            item = newItem;
        }
        else if(item == newItem && newItem.currentItemCount < 10 && newItem.itemClass == 2)
        {
            slotCount++;
            itemCountText.text = newItem.currentItemCount.ToString();
        }
        else if (item == newItem && newItem.currentItemCount >= 10 && newItem.itemClass == 2)
        {
            item = newItem;
        }
    }

    public void RemoveItem()
    {
        slotCount = 0;
        item.currentItemCount = 0;
        if (item.currentItemCount <= 0)
        {
            item = null;
            itemCountText.text = item.currentItemCount.ToString();
        }
    }

    public void UpdateUI()
    {
        if (item != null)
        {
            itemImage.sprite = item.image;
            itemImage.enabled = true;
            itemCountText.enabled = false;
            if (item.isStackable == true)
            {
                itemCountText.text = item.currentItemCount.ToString();
                itemCountText.enabled = true;
            }
        }
        else
        {
            itemImage.enabled = false;
            itemCountText.enabled = false;
        }
    }
    
    

}
