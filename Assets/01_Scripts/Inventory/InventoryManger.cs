using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class InventoryManger : SingletonManager<InventoryManger>
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
        int random = Random.Range(0, allItems.Count);
        // Random.
        return allItems[random];
    }

    public float DropProbabillity(ItemBase item)
    {
        float probabillity = item.dropPercent / 100;
        
        return probabillity;
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
                if (slot.item == newItem && !slot.IsFull())
                {
                    myItems.Add(newItem);
                    newItem.currentItemCount++;
                    slot.slotCount++;
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
                    slot.slotCount++;
                    newItem.isHave = true;
                    Debug.Log($"인벤토리 내 아이템 개수: {myItems.Count}");

                    break;
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
                slot.item.currentItemCount -= slot.slotCount;
            }

            else if (slot.item.itemClass == 1)
            {
                slot.item.currentItemCount--;
            }
            
            Debug.Log($"삭제된 아이템: {slot.item.name}, 삭제되고 남은 아이템 개수: {slot.item.currentItemCount}");
            myItems.Remove(slot.item);
            slot.RemoveItem();
            SlotArray();
        }
    }

    public void SlotArray()
    {
        List<(ItemBase item, int count)> items = new List<(ItemBase, int)>();

        foreach (InventorySlot slot in slots)
        {
            if (slot.item != null)
            {
                items.Add((slot.item, slot.slotCount));
                slot.RemoveItem();
            }
        }

        foreach (var itemInfo in items)
        {
            foreach (InventorySlot slot in slots)
            {
                if (slot.item == null)
                {
                    slot.AddItem(itemInfo.item);
                    slot.slotCount = itemInfo.count;
                    break;
                }
            }
        }
    }
}
