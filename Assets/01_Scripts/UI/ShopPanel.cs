using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ShopPanel : MonoBehaviour
{
	public GameObject shopPanel;
	public Text sellPriceText;
	public Text currentGoldText;
	public List<Slot> holdSlots; // 보유 물품 슬롯들
	public List<Slot> sellSlots; // 판매할 물품 슬롯들
	public Button sellButton;
	public Button closeButton;

	private bool isSellButtonClicked = false;
	[HideInInspector] public float sellPrice = 0;
	[HideInInspector] public Dictionary<Slot, Slot> originalSlotState =
		new Dictionary<Slot, Slot>();
	private Slot sellSlot;
	private Dictionary<Slot, ItemState> itemStates =
		new Dictionary<Slot, ItemState>();

	private struct ItemState
	{
		public int originalCount;
		public bool wasInteractable;
		public float originalAlpha;
	}

	private void Awake()
	{
		ShopButtonClicked();
		SaveOriginalState();
	}

	private void SaveOriginalState()
	{
		foreach (Slot slot in holdSlots)
		{
			itemStates[slot] = new ItemState
			{
				originalCount = slot.slotCount,
				wasInteractable = slot.canvasGroup.interactable,
				originalAlpha = slot.canvasGroup.alpha
			};
		}
	}

	public void RestoreOriginalState()
	{
		if (!isSellButtonClicked)
		{
			foreach (Slot slot in holdSlots)
			{
				if (itemStates.ContainsKey(slot))
				{
					var state = itemStates[slot];
					slot.slotCount = state.originalCount;
					slot.canvasGroup.interactable = true;
					slot.canvasGroup.alpha = 1f;
				}
			}

			foreach (Slot slot in sellSlots)
			{
				slot.RemoveItem();
				slot.slotCount = 0;
			}

			originalSlotState.Clear();
			sellPrice = 0;
			UpdateHoldSlots();
		}
		else
		{
			isSellButtonClicked = false;
			sellPrice = 0;
			foreach (Slot slot in holdSlots)
			{
				if (itemStates.ContainsKey(slot))
				{
					slot.canvasGroup.interactable = true;
					slot.canvasGroup.alpha = 1f;
				}
			}

			UpdateHoldSlots();
		}
	}

	private void Update()
	{
		SetGold();
	}

	private void UpdateHoldSlots()
	{
		foreach (Slot holdSlot in holdSlots)
		{
			holdSlot.RemoveItem();
		}

		List<Slot> inventorySlots = UIManager.Instance.inventoryManagerSystem.slots;
		for (int i = 0; i < inventorySlots.Count && i < holdSlots.Count; i++)
		{
			if (inventorySlots[i].item != null)
			{
				holdSlots[i].AddItem(inventorySlots[i].item);
				holdSlots[i].slotCount = inventorySlots[i].slotCount;
			}
		}
	}

	private void ShopButtonClicked()
	{
		closeButton.onClick.AddListener(CloseButtonClick);
		sellButton.onClick.AddListener(sellButtonClick);
	}

	private void sellButtonClick()
	{
		bool isItemToSell = false;
		foreach (Slot slot in sellSlots)
		{
			if (slot.item != null)
			{
				isItemToSell = true;
				break;
			}
		}

		if (!isItemToSell)
		{
			UIManager.Instance.popUp.PopUpOpen("판매할 수 있는\n아이템이 없습니다.",
				() => UIManager.Instance.popUp.PopUpClose());
			return;
		}

		bool arraySlot = false;

		foreach (Slot sellSlot in sellSlots)
		{
			if (sellSlot.item != null)
			{
				if (originalSlotState.TryGetValue(sellSlot, out Slot originalSlot))
				{
					int holdSlotIndex = holdSlots.IndexOf(originalSlot);
					Slot slot =
						UIManager.Instance.inventoryManagerSystem.slots[holdSlotIndex];

					if (sellSlot.item.itemClass == 2)
					{
						sellSlot.item.currentItemCount -= sellSlot.slotCount;


						if (slot != null)
						{
							if (slot.slotCount <= sellSlot.slotCount)
							{
								slot.RemoveItem();
								slot.slotCount = 0;
								arraySlot = true;
							}
							else
							{
								slot.slotCount -= sellSlot.slotCount;
							}
						}
					}
					else if (sellSlot.item.itemClass == 1)
					{
						sellSlot.item.currentItemCount--;
						if (slot != null)
						{
							slot.RemoveItem();
							slot.slotCount = 0;
							arraySlot = true;
						}
					}

					sellSlot.RemoveItem();
					sellSlot.slotCount = 0;
				}
			}
		}

		Player player = GameObject.Find($"{FirebaseManager.Instance.CurrentUserData.user_Name}").GetComponent<Player>();
		player.AddGold(sellPrice);

		sellPrice = 0;
		isSellButtonClicked = true;

		if (arraySlot)
		{
			InventoryManager.Instance.SlotArray();
		}

		Debug.Log($"현재 골드: {FirebaseManager.Instance.CurrentUserData.user_Gold}");
		UpdateHoldSlots();
		InventoryManager.Instance.UpdateSlotData();
		UIManager.Instance.CloseShopPanel();
	}

	private void CloseButtonClick()
	{
		UIManager.Instance.CloseShopPanel();
	}

	public void TryOpenShop()
	{
		if (!UIManager.Instance.isShopActive)
		{
			UIManager.Instance.OpenShopPanel();
			UpdateHoldSlots();
		}
		else
		{
			UIManager.Instance.CloseShopPanel();
		}
	}

	private void SetGold()
	{
		if (FirebaseManager.Instance.CurrentUserData != null)
		{
			float gold = FirebaseManager.Instance.CurrentUserData.user_Gold;

			currentGoldText.text = "보유 골드: " + gold.ToString();
			sellPriceText.text = "판매 가격: " + sellPrice.ToString();
		}

		else
		{
			return;
		}
	}
}