using UnityEngine;
using UnityEngine.SceneManagement;
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

	/*[HideInInspector]*/
	public bool isInventoryActive = false;

	/*[HideInInspector]*/
	public bool isItemInfoActive = false;

	/*[HideInInspector]*/
	public bool isShopActive = false;

	// Chat
	public ChatPanel chatPanel;

	public Button chatButton;

	/*[HideInInspector]*/
	public bool isChatActive = false;

	// Party
	public PartyPanel partyPanel;
	public PartyCreatePanel partyCreatePanel;
	public PartyMemberPanel partyMemberPanel;

	public Button partyButton;

	/*[HideInInspector]*/
	public bool isPartyActive = false;

	/*[HideInInspector]*/
	public bool isPartyCreateActive = false;

	/*[HideInInspector]*/
	public bool isPartyMemberActive = false;

	// Dungeon
	public DungeonPanel dungeonPanel;

	/*[HideInInspector]*/
	public bool isDungeonActive = false;

	//Option
	public OptionPanel optionPanel;

	/*[HideInInspector]*/
	public bool isOptionActive = false;

	//Skill UI
	public GameObject InGamePanel;
	public GameObject skillUIPanel;
	public bool isSkillPanelActive = false;

	//Mobile UI
	public SkillUIManager skillUI;
	public MobileUI mobile;

	protected override void Awake()
	{
		base.Awake();
		itemInfo = FindObjectOfType<ItemInfoPanel>();
		inventory = FindObjectOfType<InventoryPanel>();
		shop = FindObjectOfType<ShopPanel>();
		equipment = FindObjectOfType<EquipmentPanel>();
		inventoryMangerSystem = FindObjectOfType<InventoryManger>();
		popUp = FindObjectOfType<PopUp>();
		skillUI = FindObjectOfType<SkillUIManager>();
		popUp.PopUpClose();
		CloseInventoryPanel();
		CloseShopPanel();
		CloseItemInfoPanel();
	}

	private void Update()
	{
		if (!skillUIPanel.activeSelf) isSkillPanelActive = false;
	}

	public void MobileSetting()
	{
		InGamePanel.SetActive(true);
		mobile.gameObject.SetActive(true);
	}

	#region Inventory

	public void OpenInventoryPanel()
	{
		if (!IsAnyPanelOpen())
		{
			isInventoryActive = true;
			inventory.panel.SetActive(true);
			currentPanel = inventory.panel;
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

	#region Chat

	public void ChatButtonClick()
	{
		if (IsAnyPanelOpen() || IsPopUpOpen()) return;
		chatPanel.gameObject.SetActive(true);
		chatButton.gameObject.SetActive(false);
		currentPanel = chatPanel.gameObject;
		isChatActive = true;
	}

	public void CloseChatPanel()
	{
		chatPanel.gameObject.SetActive(false);
		currentPanel = null;
		isChatActive = false;
	}

	#endregion

	#region Party Management

	public void OnPartyButtonClick()
	{
		if (IsAnyPanelOpen() || IsPopUpOpen()) return;

		// 파티 업데이트
		FirebaseManager.Instance.UpdatePartyAndList();
		// party member 확인 후 파티에 가입되어 있는 지 확인 후 맞는 패널을 열어야 함.
		// 파티 없을 때
		if (FirebaseManager.Instance.CurrentUserData.user_CurrentParty == "")
		{
			partyPanel.gameObject.SetActive(true);
			partyMemberPanel.gameObject.SetActive(false);
			isPartyActive = true;
		}
		// 파티 있을 때
		else
		{
			partyPanel.gameObject.SetActive(false);
			partyMemberPanel.gameObject.SetActive(false);
			partyMemberPanel.gameObject.SetActive(true);
			isPartyMemberActive = true;
		}

		// party create panel은 비활성화 해야함
		partyCreatePanel.gameObject.SetActive(false);
	}

	public void ClosePartyPanel()
	{
		partyPanel.gameObject.SetActive(false);
		partyMemberPanel.gameObject.SetActive(false);
		isPartyActive = false;
		isPartyMemberActive = false;
		ClosePartyCreatePanel();
	}

	public void OpenPartyCreatePanel()
	{
		partyCreatePanel.gameObject.SetActive(true);
		isPartyCreateActive = true;
	}

	public void ClosePartyCreatePanel()
	{
		partyCreatePanel.gameObject.SetActive(false);
		isPartyCreateActive = false;
	}

	#endregion

	#region Dungeon

	public void OpenDungeonPanel()
	{
		if (IsAnyPanelOpen() || IsPopUpOpen()) return;
		FirebaseManager.Instance.UpdatePartyAndList();
		dungeonPanel.gameObject.SetActive(true);
		isDungeonActive = true;
	}

	public void CloseDungeonPanel()
	{
		dungeonPanel.gameObject.SetActive(false);
		isDungeonActive = false;
	}

	#endregion

	#region Skill

	public void OpenSkillPanel()
	{
		if (IsAnyPanelOpen() || IsPopUpOpen()) return;
		skillUIPanel.gameObject.SetActive(true);
		isSkillPanelActive = true;
	}

	public void CloseSkillPanel()
	{
		skillUIPanel.gameObject.SetActive(false);
		isSkillPanelActive = false;
	}

	#endregion

	public void CloseAllPanels()
	{
		CloseInventoryPanel();
		CloseShopPanel();
		CloseItemInfoPanel();
		CloseOptionPanel();
		CloseChatPanel();
		ClosePartyPanel();
		ClosePartyCreatePanel();
		CloseDungeonPanel();
		CloseSkillPanel();
	}

	public bool IsAnyPanelOpen()
	{
		return isInventoryActive || isItemInfoActive || isShopActive ||
		       isOptionActive || isChatActive || isPartyActive ||
		       isPartyCreateActive || isPartyMemberActive || isDungeonActive ||
		       isSkillPanelActive;
	}

	public bool IsPopUpOpen()
	{
		return popUp.gameObject.activeSelf;
	}
}