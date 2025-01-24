using System;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPanel : MonoBehaviour
{
	public CanvasGroup invenCanvasGroup;
	public GameObject panel;
	public GameObject notInteractablePanel;
	
	public Button invenCloseButton;
	public Text currentGoldText;

	private void Awake()
	{
		ButtonOnClick();
	}

	private void Start()
	{
		notInteractablePanel.SetActive(false);
	}

	private void Update()
	{
		InteractableController();
		SetCurrentGold();
	}

	private void ButtonOnClick()
	{
		invenCloseButton.onClick.AddListener(InvenCloseButtonClick);
	}

	private void InvenCloseButtonClick()
	{
		UIManager.Instance.CloseInventoryPanel();
	}

	public void TryOpenInventory()
	{
		if (!UIManager.Instance.isInventoryActive)
		{
			UIManager.Instance.OpenInventoryPanel();
		}
		else
		{
			UIManager.Instance.CloseInventoryPanel();
		}
	}

	private void InteractableController()
	{
		if (UIManager.Instance.itemInfo.itemInfoPanel.activeSelf)
		{
			notInteractablePanel.SetActive(true);
			invenCanvasGroup.interactable = false;
		}
		else if (!UIManager.Instance.itemInfo.itemInfoPanel.activeSelf)
		{
			notInteractablePanel.SetActive(false);
			invenCanvasGroup.interactable = true;
		}
	}

	private void SetCurrentGold()
	{
		if (FirebaseManager.Instance.CurrentUserData != null)
		{
			// 서버로 보낼 땐 이 코드 사용
			float gold = FirebaseManager.Instance.CurrentUserData.user_Gold;
			currentGoldText.text = "보유 골드: " + gold.ToString();
		}
	}
}