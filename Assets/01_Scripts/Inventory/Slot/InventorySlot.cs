using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum SlotType
{
    Inventory,
    Shop,
    ShopSell
}
public class InventorySlot : MonoBehaviour
{
    public ItemBase item;
    public Image itemImage;
    public Button itemButton;
    public Text itemCountText;
    
    private int count = 0;
    private SlotType slotType;

    private void Awake()
    {
        itemButton.onClick.AddListener(itemButtonClick);
    }

    private void Start()
    {
        SetSlotType();
    }

    private void SetSlotType()
    {
        if (transform.parent.name == "Inventory")
        {
            slotType = SlotType.Inventory;
        }
        else if (transform.parent.name == "HoldSlot")
        {
            slotType = SlotType.Shop;
        }
        else if (transform.parent.name == "SellSlot")
        {
            slotType = SlotType.ShopSell;
        }
    }
    private void itemButtonClick()
    {
        if (slotType == SlotType.Inventory)
        {
            UIManager.Instance.OpenItemInfoPanel();
            UIManager.Instance.SetItemInfoScreen(item);
        }
        else if (slotType == SlotType.Shop)
        {
            
        }
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
            count = 1;
        }
        else if(item == newItem && count < 10 && newItem.itemClass == 2)
        {
            count++;
            itemCountText.text = count.ToString();
        }

        else
        {
            {
                Debug.Log("인벤토리가 가득차 더이상 아이템을 추가할 수 없습니다");
                return;
            }
        }
    }

    public void RemoveItem()
    {
        count = 0;
        if (count <= 0)
        {
            item = null;
        }
    }

    public bool IsFull()
    {
        return count >= 10;
    }
    public void UpdateUI()
    {
        if (item != null)
        {
            itemImage.sprite = item.image;
            itemImage.enabled = true;
            itemCountText.enabled = false;
            if (item.itemClass == 2)
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
