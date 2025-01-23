using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : SingletonManager<UIManager>
{
	public GameObject currentPanel;
	
	public ItemInfoPanel itemInfo;
	public InventoryPanel inventory;
	public ShopPanel shop;
	public EquipmentPanel equipment;
	public InventoryManger inventoryMangerSystem;
	public PopUp popUp;
	[HideInInspector] public bool isInventoryActive = false;
	[HideInInspector] public bool isItemInfoActive = false;
	[HideInInspector] public bool isShopActive = false;

	// Chat
	public ChatPanel chatPanel;
	public Button chatButton;

	// Party
	public PartyPanel partyPanel;
	public PartyCreatePanel partyCreatePanel;
	public PartyMemberPanel partyMemberPanel;
	public Button partyButton;
	public DungeonPanel dungeonPanel;

	//Option
	public OptionPanel optionPanel;
	[HideInInspector] public bool isOptionActive = false;
	
	//Skill UI
	[FormerlySerializedAs("InGamePanel")] public GameObject InGamePanel;

	protected override void Awake()
	{
		base.Awake();
		itemInfo = FindObjectOfType<ItemInfoPanel>();
		inventory = FindObjectOfType<InventoryPanel>();
		shop = FindObjectOfType<ShopPanel>();
		equipment = FindObjectOfType<EquipmentPanel>();
		inventoryMangerSystem = FindObjectOfType<InventoryManger>();
		popUp = FindObjectOfType<PopUp>();
		popUp.PopUpClose();
		CloseInventoryPanel();
		CloseShopPanel();
		CloseItemInfoPanel();
	}

	#region Inventory

	public void OpenInventoryPanel()
	{
		if (!IsAnyPanelOpen())
		{
			isInventoryActive = true;
			inventory.panel.SetActive(true);
			currentPanel=inventory.panel;
		}
	}

	public void CloseInventoryPanel()
	{
		isInventoryActive = false;
		inventory.panel.SetActive(false);
		CloseItemInfoPanel();
		currentPanel = null;
	}

	#endregion
	
	#region Shop

	public void OpenShopPanel()
	{
		if (!IsAnyPanelOpen())
		{
			isShopActive = true;
			shop.shopPanel.SetActive(true);
			currentPanel = shop.shopPanel;
		}
	}

	public void CloseShopPanel()
	{
		isShopActive = false;
		shop.RestoreOriginalState();
		shop.shopPanel.SetActive(false);
		currentPanel = null;
	}

	#endregion

	#region ItemInfo

	public void OpenItemInfoPanel()
	{
		isItemInfoActive = true;
		itemInfo.itemInfoPanel.SetActive(true);
		currentPanel = itemInfo.itemInfoPanel;
	}

	public void CloseItemInfoPanel()
	{
		isItemInfoActive = false;
		itemInfo.itemInfoPanel.SetActive(false);
		currentPanel = null;
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

		itemInfo.itemName.text = item.itemName.ToString();
		itemInfo.itemDescription.text = item.description.ToString();
		itemInfo.itemImage.sprite = item.image;
	}

	#endregion
	
	#region Option
	public void OpenOptionPanel()
	{
		if (!IsAnyPanelOpen())
		{
			isOptionActive = true;
			optionPanel.gameObject.SetActive(true);
			currentPanel = optionPanel.gameObject;
		}
	}

	public void CloseOptionPanel()
	{
		isOptionActive = false;
		optionPanel.gameObject.SetActive(false);
		currentPanel = null;
	}

	#endregion
	
	public bool IsAnyPanelOpen()
	{
		return isInventoryActive || isItemInfoActive || isShopActive || isOptionActive;
	}

	#region Chat

	public void ChatButtonClick()
	{
		chatPanel.gameObject.SetActive(true);
		chatButton.gameObject.SetActive(false);
		currentPanel = chatPanel.gameObject;
	}

	#endregion

	#region Party Management

	public void OnPartyButtonClick()
	{
		// 파티 업데이트 
		FirebaseManager.Instance.UpdatePartyAndList();
		// party member 확인 후 파티에 가입되어 있는 지 확인 후 맞는 패널을 열어야 함.
		if (FirebaseManager.Instance.CurrentUserData.user_CurrentParty == "")
		{
			partyPanel.gameObject.SetActive(true);
			partyMemberPanel.gameObject.SetActive(false);
		}
		else
		{
			partyPanel.gameObject.SetActive(false);
			partyMemberPanel.gameObject.SetActive(false);
			partyMemberPanel.gameObject.SetActive(true);
		}

		// party create panel은 비활성화 해야함
		partyCreatePanel.gameObject.SetActive(false);
	}

	public void OpenPartyCreatePanel()
	{
		partyCreatePanel.gameObject.SetActive(true);
	}

	public void ClosePartyPanel()
	{
		partyPanel.gameObject.SetActive(false);
	}

	public void ClosePartyCreatePanel()
	{
		partyCreatePanel.gameObject.SetActive(false);
	}

	#endregion

	public void OpenDungeonPanel()
	{
		FirebaseManager.Instance.UpdatePartyAndList();
		dungeonPanel.gameObject.SetActive(true);
	}
}