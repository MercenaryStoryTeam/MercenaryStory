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

    public ItemBase RandomDropItems()
    { 
        int random = UnityEngine.Random.Range(0, allItems.Count);
        return allItems[random];
    }
    public void AddItemToInventory(ItemBase newItem)
    {
        foreach (var slot in slots)
        {
            if (slot.item == null)
            {
                    slot.AddItem(newItem);
                    myItems.Add(newItem);
                    newItem.currentItemCount++;
                    Debug.Log($"인벤토리 내 아이템 개수: {myItems.Count}");
                
                    break;
                }

            
            
        }
    }

}
