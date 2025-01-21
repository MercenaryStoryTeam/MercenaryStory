using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;

public class InventoryManger : SingletonManager<InventoryManger>
{
    public List<ItemBase> myItems;
    public List<ItemBase> allItems;
    public List<InventorySlot> slots;

    public ItemBase basicWeapon; //캐릭터 생성시 인벤토리에 추가될 양손검 아이템
    public ItemBase basicEquipWeapon; //캐릭터 생성시 장착 중일 한손검 + 방패 아이템

    public SlotData currentSlotData;
    
    protected override void Awake()
    {
        base.Awake();
        
    }

    private void Update()
    {

    }

    public SlotData SetBasicItem(ItemBase item, ItemBase setItem)
    {
        myItems.Add(setItem);
        setItem.currentItemCount++;
        AddItemToInventory(item);
        currentSlotData = new SlotData(item.id, 1);
        return currentSlotData;
    }
    
    //테스트용 드롭 구현
    public ItemBase RandomDropItems()
    {
        int random = Random.Range(0, allItems.Count);
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
                    Debug.Log($"추가한 {newItem.name}의 개수: {newItem.currentItemCount}");
                    Debug.Log($"현재 가지고 있는 아이템 개수: {myItems.Count}");

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
                    UpdateSlotData();
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
                    UpdateSlotData();
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
    
    // 서버에 올릴 때 사용하면 된다.
    // ServerManager.JoinOrCreatePersistentRoom();에서
    public bool UpdateSlotData()
    {
        Debug.Log("UpdateSlotData 시작");
    
        // FirebaseManager 체크
        if (FirebaseManager.Instance == null)
        {
            Debug.LogError("FirebaseManager.Instance가 null입니다");
            return false;
        }
    
        // CurrentUserData 체크
        if (FirebaseManager.Instance.CurrentUserData == null)
        {
            Debug.LogError("CurrentUserData가 null입니다");
            return false;
        }
    
        // slots 체크
        if (slots == null)
        {
            Debug.LogError("slots가 null입니다");
            return false;
        }
    
        // user_Inventory 체크
        if (FirebaseManager.Instance.CurrentUserData.user_Inventory == null)
        {
            Debug.LogError("user_Inventory가 null입니다");
            FirebaseManager.Instance.CurrentUserData.user_Inventory = new List<SlotData>();
        }

        try 
        {
            UserData currentUserData = FirebaseManager.Instance.CurrentUserData;
            Debug.Log($"현재 인벤토리 아이템 수: {currentUserData.user_Inventory.Count}");
        
            currentUserData.user_Inventory.Clear();

            foreach (var slot in slots)
            {
                if (slot == null)
                {
                    Debug.LogWarning("slot이 null입니다");
                    continue;
                }

                if(slot.item != null)
                {
                    SlotData slotData = new SlotData(slot.item.id, slot.slotCount);
                    currentUserData.user_Inventory.Add(slotData);
                    Debug.Log($"슬롯 데이터 추가: ItemID={slot.item.id}, Count={slot.slotCount}");
                }
            }

            FirebaseManager.Instance.UploadCurrnetInvenData("user_Inventory", currentUserData.user_Inventory);
            Debug.Log("Firebase 업로드 요청 완료");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"인벤토리 업데이트 실패: {e.Message}");
            return false;
        }
    }
}
