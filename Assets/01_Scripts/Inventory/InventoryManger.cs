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

    public ItemBase basicWeapon; //캐릭터 생성시 인벤토리에 추가될 양손검 아이템
    
    protected override void Awake()
    {
        base.Awake();
    }

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
    
    // 서버에 올릴 때 사용하면 된다.
    // ServerManager.JoinOrCreatePersistentRoom();에서
    public void UpdateSlotData()
    {
        // 제대로 될 지 모르겠다. 디버그 필수!!!!
        UserData currentUserData = FirebaseManager.Instance.CurrentUserData;
        currentUserData.user_Inventory.Clear();
        // SlotData slotData = new SlotData();
        // foreach (var slot in slots)
        // {
        //     slotData.item_Id = slot.item.id;
        //     slotData.item_Stack = slot.slotCount;
        //     currentUserData.user_Inventory.Add(slotData);
        // }
    }
}
