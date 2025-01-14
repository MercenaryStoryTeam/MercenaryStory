using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonManager<UIManager>
{
	public ItemInfoPanel itemInfo;
	public InventoryPanel inventory;
	public ShopPanel shop;
	public EquipmentPanel equipment;
	public Inventory inventorySystem;
	public PopUp popUp;
	public InventorySlot slot;
	[HideInInspector] public bool isInventoryActive = false;
	[HideInInspector] public bool isItemInfoActive = false;
	[HideInInspector] public bool isShopActive = false;

	// Chat
	public ChatPanel chatPanel;
	public Button chatButton;

	protected override void Awake()
	{
		base.Awake();
		itemInfo = FindObjectOfType<ItemInfoPanel>();
		inventory = FindObjectOfType<InventoryPanel>();
		shop = FindObjectOfType<ShopPanel>();
		equipment = FindObjectOfType<EquipmentPanel>();
		inventorySystem = FindObjectOfType<Inventory>();
		popUp = FindObjectOfType<PopUp>();
	}

	#region Inventory

	public void OpenInventoryPanel()
	{
		if (IsAnyPanelOpen())
		{
			CloseShopPanel();
		}

		isInventoryActive = true;
		inventory.panel.SetActive(true);
	}

	public void CloseInventoryPanel()
	{
		isInventoryActive = false;
		inventory.panel.SetActive(false);
	}

	#endregion

	#region Shop

	public void OpenShopPanel()
	{
		if (IsAnyPanelOpen())
		{
			CloseInventoryPanel();
			CloseItemInfoPanel();
		}

		isShopActive = true;
		shop.shopPanel.SetActive(true);
	}

	public void CloseShopPanel()
	{
		foreach (InventorySlot slot in shop.sellSlots)
		{
			slot.RemoveItem();
		}

		foreach (InventorySlot slot in shop.holdSlots)
		{
			//행동 초기화 로직 추가
			slot.canvasGroup.alpha = 1f;
		}

		isShopActive = false;
		shop.shopPanel.SetActive(false);
	}

	#endregion

	#region ItemInfo

	public void OpenItemInfoPanel()
	{
		isItemInfoActive = true;
		itemInfo.itemInfoPanel.SetActive(true);
	}

	public void CloseItemInfoPanel()
	{
		isItemInfoActive = false;
		itemInfo.itemInfoPanel.SetActive(false);
	}

	public void SetItemInfoScreen(ItemBase item)
	{
		if (item.itemClass == 1)
		{
			itemInfo.firstOptionButton.enabled = true;
			itemInfo.secondOptionButton.enabled = true;
			itemInfo.secondOption.SetActive(true);
			itemInfo.firstOptionText.text = "장착";
		}

		if (item.itemClass == 2)
		{
			itemInfo.firstOptionButton.enabled = false;
			itemInfo.secondOptionButton.enabled = true;
			itemInfo.secondOption.SetActive(true);
			itemInfo.firstOptionText.text = item.currentItemCount + "개";
		}

		if (item.itemClass == 3)
		{
			itemInfo.firstOptionButton.enabled = false;
			itemInfo.secondOption.SetActive(false);
			itemInfo.firstOptionText.text = item.currentItemCount + "개";
		}

		itemInfo.itemName.text = item.name.ToString();
		itemInfo.itemDescription.text = item.description.ToString();
		itemInfo.itemImage.sprite = item.image;
	}

	#endregion

	// //AnyPanelOpen에 다른 패널 추가되면 사용 예정
	// //현재 앞 순서 if문만 적용되고있음
	// public void CloseAllPanel()
	// {        
	//     if (isShopActive)
	//     {
	//         CloseInventoryPanel();
	//     }
	//     if (isInventoryActive)
	//     {
	//         CloseShopPanel();
	//     }
	//     
	// }

	public bool IsAnyPanelOpen()
	{
		return isInventoryActive || isItemInfoActive || isShopActive;
	}

	#region Chat

	public void ChatButtonClick()
	{
		chatPanel.gameObject.SetActive(true);
	}

	#endregion
}