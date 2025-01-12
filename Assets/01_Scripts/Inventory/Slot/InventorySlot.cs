using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

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
        itemButton.onClick.AddListener(OnSlotClick);
        SetSlotType();
    }

    private void Update()
    {
        UpdateUI();
    }

    private void SetSlotType()
    {
        // 슬롯 부모 오브젝트 이름에 따라 슬롯 타입 변경
        if (transform.parent.name == "Slots")
        {
            slotType = SlotType.Inventory;
        }
        else if (transform.parent.name == "HoldSlots")
        {
            slotType = SlotType.Shop;
        }
        else if (transform.parent.name == "SellSlots")
        {
            slotType = SlotType.ShopSell;
            itemButton.enabled = false;
        }
    }

    private void OnSlotClick()
    {
        if (item == null) return;

        switch (slotType)
        {
            case SlotType.Inventory:
                UIManager.Instance.OpenItemInfoPanel();
                UIManager.Instance.itemInfo.SetCurrentSlot(this);
                UIManager.Instance.SetItemInfoScreen(item);
                break;

            case SlotType.Shop:
                MoveItemToSellSlot();
                break;
        }
    }

    private void MoveItemToSellSlot()
    {
        if (item == null || (item.itemClass == 2 && item.currentItemCount <= 0))
        {
            return;
        }

        if (item.itemClass == 3)  
        {
            return;
        }

        List<InventorySlot> sellSlots = UIManager.Instance.shop.sellSlots;

        if (item.itemClass == 1) 
        {
            bool existingEquip = false;
            foreach (InventorySlot slot in sellSlots)
            {
                if (slot.item != null && slot.item.name == item.name)
                {
                    existingEquip = true;
                    break;
                }
            }
            
            if (existingEquip)
            {
                return;  
            }
        }

        if (item.itemClass == 2) 
        {
            InventorySlot existingSellSlot = null;
            foreach (InventorySlot slot in sellSlots)
            {
                if (slot.item != null && slot.item.name == item.name)
                {
                    existingSellSlot = slot;
                    break;
                }
            }

            if (existingSellSlot != null)
            {
                existingSellSlot.item.currentItemCount++;
                item.currentItemCount--;
                if (item.currentItemCount <= 0)
                {
                    RemoveItem();
                }
            }
            else
            {
                foreach (InventorySlot sellSlot in sellSlots)
                {
                    if (sellSlot.item == null)
                    {
                        ItemBase sellItem = Instantiate(item);
                        sellItem.currentItemCount = 1;
                        sellSlot.AddItem(sellItem);
                        
                        item.currentItemCount--;
                        if (item.currentItemCount <= 0)
                        {
                            RemoveItem();
                        }
                        break;
                    }
                }
            }
        }

        else  
        {
            foreach (InventorySlot sellSlot in sellSlots)
            {
                if (sellSlot.item == null)
                {
                    sellSlot.AddItem(item);
                    RemoveItem();
                    break;
                }
            }
        }
    }

    public void AddItem(ItemBase newItem)
    {
        item = newItem;
        UpdateUI();
    }

    public void RemoveItem()
    {
        item = null;
        UpdateUI();
    }

    private void UpdateUI()
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
