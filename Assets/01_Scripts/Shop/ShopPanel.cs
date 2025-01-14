using Firebase.Auth;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour
{
	public GameObject shopPanel;
	public Text sellPriceText;
	public Text currentGoldText;
	public List<InventorySlot> holdSlots; // 보유 물품 슬롯들
	public List<InventorySlot> sellSlots; // 판매할 물품 슬롯들

	public Button sellButton;
	public Button closeButton;

	[HideInInspector] public float sellPrice = 0;
	[HideInInspector] public Dictionary<InventorySlot, InventorySlot> originalSlotState = new Dictionary<InventorySlot, InventorySlot>();
		
	private TestSY _testsy;
	private InventorySlot selectedSlot;
	private InventorySlot sellSlot;
	private Dictionary<InventorySlot, ItemState> itemStates = new Dictionary<InventorySlot, ItemState>();

	private struct ItemState
	{
		public int originalCount;
		public bool wasInteractable;
		public float originalAlpha;
	}

	private void Awake()
	{
		_testsy = FindObjectOfType<TestSY>();
		shopPanel.SetActive(false);
		ShopButtonClicked();
		SaveOriginalState();
	}

	private void SaveOriginalState()
	{
		foreach (InventorySlot slot in holdSlots)
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
		foreach (InventorySlot slot in holdSlots)
		{
			if (itemStates.ContainsKey(slot))
			{
				var state = itemStates[slot];
				slot.slotCount = state.originalCount;
				slot.canvasGroup.interactable = true;
				slot.canvasGroup.alpha = 1f;
			}
		}
		
		foreach (InventorySlot slot in sellSlots)
		{
			slot.RemoveItem();
			slot.slotCount = 0;
		}
		
		originalSlotState.Clear();
		sellPrice = 0;
		UpdateHoldSlots();
	}

	private void Update()
	{
		SetGold();
	}

	private void UpdateHoldSlots()
	{
		foreach (InventorySlot holdSlot in holdSlots)
		{
			holdSlot.RemoveItem();
		}

		List<InventorySlot> inventorySlots = UIManager.Instance.inventorySystem.slots;
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
		foreach(InventorySlot slot in sellSlots)
		{
            if (slot.item == null)
            {
                UIManager.Instance.popUp.PopUpOpen("판매할 수 있는\n아이템이 없습니다.",
                    () => UIManager.Instance.popUp.PopUpClose());
            }
        }
	}

	private void CloseButtonClick()
	{
		UIManager.Instance.CloseShopPanel();
	}

	public void TryOpenShop()
	{
		if (Input.GetKeyDown(KeyCode.E))
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
	}

	private void SetGold()
	{
		currentGoldText.text = "보유 골드: " + _testsy.myGold.ToString();
		sellPriceText.text = "판매 가격: " + sellPrice.ToString();
	}
}