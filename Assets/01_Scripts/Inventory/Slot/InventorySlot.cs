using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
	public CanvasGroup canvasGroup;

	[HideInInspector] public int slotCount = 0;
	private SlotType slotType;

	private void Awake()
	{
		itemButton.onClick.AddListener(OnSlotClicked);
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

	private void OnSlotClicked()
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
		if (item == null || (item.itemClass == 2 && slotCount <= 0))
		{
			return;
		}

		if (item.itemClass == 3)
		{
			UIManager.Instance.popUp.PopUpOpen("해당 아이템을\n상인이 원하지 않습니다.",
				() => UIManager.Instance.popUp.PopUpClose());
			return;
		}

		List<InventorySlot> sellSlots = UIManager.Instance.shop.sellSlots;

		if (item.itemClass == 1)
		{
			foreach (InventorySlot sellSlot in sellSlots)
			{
				if (sellSlot.item == null)
				{
					canvasGroup.alpha = 0.5f;
					canvasGroup.interactable = false;
					sellSlot.AddItem(item);
					UIManager.Instance.shop.sellPrice += item.price;
					break;
				}
			} 
		}

		if (item.itemClass == 2)
		{
			InventorySlot currentSellSlot = null;
			foreach (InventorySlot slot in sellSlots)
			{
				if (slot.item != null && 
					slot.item.name == item.name && 
					UIManager.Instance.shop.originalSlotState.ContainsKey(slot) &&
					UIManager.Instance.shop.originalSlotState[slot] == this)
				{
					currentSellSlot = slot;
					break;
				}
			}

			if (currentSellSlot != null)
			{
				currentSellSlot.slotCount++;
				slotCount--;
				UIManager.Instance.shop.sellPrice += item.price;
				canvasGroup.alpha = 0.5f;

				if (slotCount <= 0)
				{
					canvasGroup.interactable = false;
				}
			}
			else
			{
				foreach (InventorySlot sellSlot in sellSlots)
				{
					if (sellSlot.item == null)
					{
						sellSlot.AddItem(item);
						sellSlot.slotCount = 1;
						slotCount--;
						UIManager.Instance.shop.sellPrice += item.price;
						canvasGroup.alpha = 0.5f;
						UIManager.Instance.shop.originalSlotState[sellSlot] = this;

						if (slotCount <= 0)
						{
							canvasGroup.interactable = false;
						}
						break;
					}
				}
			}
		}
	}

	public void AddItem(ItemBase newItem)
	{
		item = newItem;
	}

	public void RemoveItem()
	{
		item = null;
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
				itemCountText.text = slotCount.ToString();
				itemCountText.enabled = true;
			}
		}
		else
		{
			itemImage.enabled = false;
			itemCountText.enabled = false;
		}
	}

	public bool IsFull()
	{
		if (slotCount >= 10)
		{
			return true;
		}

		return false;
	}
}