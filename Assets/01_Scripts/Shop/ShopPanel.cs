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

	[HideInInspector] public float sellPrice;

	private TestSY _testsy;
	private InventorySlot selectedSlot;
	private InventorySlot sellSlot;

	private void Awake()
	{
		_testsy = FindObjectOfType<TestSY>();
		shopPanel.SetActive(false);
		ShopButtonClicked();
	}

	private void Update()
	{
		SetGold();
		UpdateHoldSlots();
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
		bool isItemInSellSlot = false;

		foreach (InventorySlot slot in sellSlots)
		{
			if (slot.item != null)
			{
				isItemInSellSlot = true;
				UIManager.Instance.CloseShopPanel();
				break;
			}
		}

		if (!isItemInSellSlot)
		{
			UIManager.Instance.popUp.PopUpOpen("판매할 수 있는\n아이템이 없습니다.", 
				() => UIManager.Instance.popUp.PopUpClose());
		}
	}

	private void CloseButtonClick()
	{
		UIManager.Instance.CloseShopPanel();
	}

	public void TryOpenShop()
	{
		if (Input.GetKeyDown(KeyCode.O)) // 테스트용 키 설정
		{
			if (!UIManager.Instance.isShopActive)
			{
				UIManager.Instance.OpenShopPanel();
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