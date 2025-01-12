using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<ItemBase> myItems;
    public List<ItemBase> allItems;
    public List<InventorySlot> slots;
    
    
    private void Update()
    {

    }

    //테스트용 드롭 구현
    public ItemBase RandomDropItems()
    { 
        int random = UnityEngine.Random.Range(0, allItems.Count);
        return allItems[random];
    }

    public void AddItemToInventory(ItemBase newItem)
    {
        if (newItem.itemClass == 1 || newItem.itemClass == 3)
        {
            foreach (InventorySlot slot in slots)
            {
                if (slot.item == null)
                {
                    slot.AddItem(newItem);
                    myItems.Add(newItem);
                    newItem.currentItemCount++;
                    newItem.isHave = true;
                    Debug.Log($"인벤토리 내 아이템 개수: {myItems.Count}");

                    break;
                }
            }

        }
        
        else if (newItem.itemClass == 2)
        {
            foreach (InventorySlot slot in slots)
            {
                if (slot.item == newItem)
                {
                    myItems.Add(newItem);
                    newItem.currentItemCount++;
                    Debug.Log($"인벤토리 내 아이템 개수: {myItems.Count}");

                    return;
                }
            }

            foreach (InventorySlot slot in slots)
            {
                if (slot.item == null)
                {
                    slot.AddItem(newItem);
                    myItems.Add(newItem);
                    newItem.currentItemCount++;
                    newItem.isHave = true;
                    Debug.Log($"인벤토리 내 아이템 개수: {myItems.Count}");

                    break;
                }
            }
        }
    }

    public void RemoveItemFromInventory(ItemBase item)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot.item == item)
            {
                slot.RemoveItem();
                if (item.currentItemCount <= 0)
                {
                    item.isHave = false;
                }

                if (item.itemClass == 2)
                {
                    slot.RemoveItem();
                    myItems.Remove(item);
                }
            }
        }
    }

    public void DeleteItem(InventorySlot slot)
    {
        if (slot != null && slot.item != null)
        {
            if (slot.item.itemClass == 2)
            {
                slot.item.currentItemCount = 0;
            }
            
            myItems.Remove(slot.item);
            slot.RemoveItem();
            
            SlotArray();
        }
    }

    public void SlotArray() // 슬롯 정렬
    {
        List<ItemBase> items = new List<ItemBase>();

        foreach (InventorySlot slot in slots)
        {
            if (slot.item != null)
            {
                items.Add(slot.item);
                slot.RemoveItem();
            }
        }

        foreach (ItemBase item in items)
        {
            foreach (InventorySlot slot in slots)
            {
                if (slot.item == null)
                {
                    slot.AddItem(item);
                    break;
                }
            }
        }
    }
}
