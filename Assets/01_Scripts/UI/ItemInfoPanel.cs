using UnityEngine;
using UnityEngine.UI;

public class ItemInfoPanel : MonoBehaviour
{
	public GameObject itemInfoPanel;

	public Image itemImage;
	public Text itemName;
	public Text itemDescription;
	public Text firstOptionText;

	public Button firstOptionButton;
	public Button secondOptionButton;
	public Button closeButton;

	public GameObject secondOption;

	private InventorySlot currentSelectedSlot;
	private Equipment equipment;
	private EquipmentPanel equipPanel;

	private void Awake()
	{
		InfoButtonOnClick();
		equipPanel = FindObjectOfType<EquipmentPanel>();
	}

	private void OnEnable()
	{
		// UI가 활성화될 때마다 Equipment 찾기 시도
		TryFindEquipment();
	}

	private void TryFindEquipment()
	{
		if (equipment == null)
		{
			equipment = FindObjectOfType<Equipment>();
		}
	}

	private void InfoButtonOnClick()
	{
		firstOptionButton.onClick.AddListener(EquipButtonClick);
		secondOptionButton.onClick.AddListener(RemoveItemButtonClick);
		closeButton.onClick.AddListener(CloseButtonClick);
	}

	private void CloseButtonClick()
	{
		UIManager.Instance.CloseItemInfoPanel();
	}

	private void RemoveItemButtonClick()
	{
		InventoryManger inventoryManger = FindObjectOfType<InventoryManger>();
		inventoryManger.DeleteItem(currentSelectedSlot);
		UIManager.Instance.CloseItemInfoPanel();
	}

	public void EquipButtonClick()
	{
		TryFindEquipment();

		if (equipment != null && equipPanel != null)
		{
			equipment.SetCurrentEquip(currentSelectedSlot);
			equipPanel.SetEquipImage(currentSelectedSlot);
		}
		else
		{
			Debug.LogWarning("Equipment 또는 EquipmentPanel을 찾을 수 없습니다!");
		}

		UIManager.Instance.CloseItemInfoPanel();
	}

	public void SetCurrentSlot(InventorySlot slot)
	{
		currentSelectedSlot = slot;
	}
}