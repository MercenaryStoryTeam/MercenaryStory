using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;

public class InventoryManger : SingletonManager<InventoryManger>
{
	public List<ItemBase> allItems;
	public List<InventorySlot> slots;

	public ItemBase basicEquipWeapon;

	public SlotData currentSlotData;

	public int GetTotalItemCount(ItemBase item)
	{
		int totalCount = 0;
		foreach (InventorySlot slot in slots)
		{
			if (slot.item == item)
			{
				totalCount += slot.slotCount;
			}
		}

		return totalCount;
	}


	public void LoadInventoryFromDatabase()
	{
		if (FirebaseManager.Instance.CurrentUserData != null)
		{
			foreach (ItemBase item in allItems)
			{
				if (item.id == FirebaseManager.Instance.CurrentUserData.user_weapon_item_Id)
				{
					item.currentItemCount = 1;
				}
				else
				{
					item.currentItemCount = 0;
				}
			}

			foreach (InventorySlot slot in slots)
			{
				slot.RemoveItem();
				slot.slotCount = 0;
			}

			foreach (SlotData slotData in FirebaseManager.Instance.CurrentUserData
				         .user_Inventory)
			{
				if (slotData.item_Stack > 0)
				{
					ItemBase originalItem = allItems.Find(x => x.id == slotData.item_Id);
					if (originalItem != null)
					{
						foreach (InventorySlot slot in slots)
						{
							if (slot.item == null)
							{
								slot.AddItem(originalItem);
								slot.slotCount = slotData.item_Stack;
								originalItem.currentItemCount += slotData.item_Stack;
								break;
							}
						}
					}
				}
			}
		}
	}

	private void Update()
	{
	}

	public SlotData SetBasicItem(ItemBase item)
	{
		item.currentItemCount = 1;
		currentSlotData = new SlotData(item.id, 0);
		return currentSlotData;
	}

	public void AddItemToInventory(ItemBase newItem)
	{
		if (newItem.itemClass == 1)
		{
			foreach (InventorySlot slot in slots)
			{
				if (slot.item == null)
				{
					slot.AddItem(newItem);
					slot.slotCount = 1;
					newItem.currentItemCount++;
					UpdateSlotData();
					break;
				}
			}
		}

		else if (newItem.itemClass == 2)
		{
			bool added = false;
			foreach (InventorySlot slot in slots)
			{
				if (slot.item != null && slot.item.id == newItem.id && !slot.IsFull())
				{
					slot.slotCount++;
					newItem.currentItemCount = GetTotalItemCount(newItem);
					added = true;
					break;
				}
			}

			if (!added)
			{
				foreach (InventorySlot slot in slots)
				{
					if (slot.item == null)
					{
						slot.AddItem(newItem);
						slot.slotCount = 1;
						newItem.currentItemCount = GetTotalItemCount(newItem);
						break;
					}
				}
			}

			UpdateSlotData();
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

			Debug.Log(
				$"삭제된 아이템: {slot.item.itemName}, 삭제되고 남은 아이템 개수: {slot.item.currentItemCount}");
			slot.RemoveItem();
			UpdateSlotData();
			SlotArray();
		}
	}

	public void SlotArray()
	{
		List<(ItemBase item, int stack)> itemsToKeep = new List<(ItemBase, int)>();

		foreach (InventorySlot slot in slots)
		{
			if (slot.item != null && slot.slotCount > 0)
			{
				itemsToKeep.Add((slot.item, slot.slotCount));
			}
		}

		foreach (InventorySlot slot in slots)
		{
			slot.RemoveItem();
			slot.slotCount = 0;
		}

		int currentSlot = 0;
		foreach (var itemInfo in itemsToKeep)
		{
			if (currentSlot < slots.Count)
			{
				slots[currentSlot].AddItem(itemInfo.item);
				slots[currentSlot].slotCount = itemInfo.stack;
				currentSlot++;
			}
		}

		UpdateSlotData();
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
			return false;
		}

		try
		{
			UserData currentUserData = FirebaseManager.Instance.CurrentUserData;

			currentUserData.user_Inventory.Clear();

			foreach (var slot in slots)
			{
				if (slot == null)
				{
					Debug.LogWarning("slot이 null입니다");
					continue;
				}

				if (slot.item != null)
				{
					SlotData slotData = new SlotData(slot.item.id, slot.slotCount);
					currentUserData.user_Inventory.Add(slotData);
				}
			}

			FirebaseManager.Instance.UploadCurrnetInvenData("user_Inventory",
				currentUserData.user_Inventory);
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