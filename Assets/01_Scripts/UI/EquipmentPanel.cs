using UnityEngine;
using UnityEngine.UI;

public class EquipmentPanel : MonoBehaviour
{
	public GameObject panelCharacter; // UI용 캐릭터 이미지를 가지고 있는 부모 오브젝트
	public Image currentEquipImage;
	public GameObject firstCharacter;
	public GameObject secondCharacter;
	public GameObject thirdCharacter;
	public ItemBase currentItem; //현재 장착중인 아이템. 플레이어 스크립트에 넣어야할드 ㅅ


	private void Awake()
	{
		firstCharacter.SetActive(false);
		secondCharacter.SetActive(false);
		thirdCharacter.SetActive(false);
	}

	private void Start()
	{
		// 캐릭터 외형 설정
		switch (FirebaseManager.Instance.CurrentUserData.user_Appearance)
		{
			case 1:
				firstCharacter.SetActive(true);
				break;
			case 2:
				secondCharacter.SetActive(true);
				break;
			case 3:
				thirdCharacter.SetActive(true);
				break;
		}

		// 저장된 장비 불러오기
		int savedWeaponId = FirebaseManager.Instance.CurrentUserData.user_weapon_item_Id;
		if (savedWeaponId != 0)
		{
			currentItem = InventoryManger.Instance.allItems.Find(x => x.id == savedWeaponId);
			if (currentItem != null)
			{
				currentEquipImage.sprite = currentItem.image;

				DestroyCurrentEquipments();

				if (currentItem.equipPrefab != null)
				{
					SetPanelSwordCharacter(currentItem);
				}
			}
		}
	}

	private void Update()
	{
		UpdateUI();
	}

	public void SetEquipImage(InventorySlot slot)
	{
		if (currentItem != null)
		{
			ItemBase beforeWeapon = currentItem;
			currentItem = slot.item;
			FirebaseManager.Instance.CurrentUserData.user_weapon_item_Id = slot.item.id;
			FirebaseManager.Instance.UploadCurrentUserData("user_weapon_item_Id", slot.item.id);
			currentEquipImage.sprite = slot.item.image;

			DestroyCurrentEquipments();
			if (slot.item.equipPrefab != null)
			{
				SetPanelSwordCharacter(slot.item);
			}

			slot.RemoveItem();
			print(
				$"현재 장착한 아이템: {currentItem.itemName}, 장착한 아이탬 개수: {currentItem.currentItemCount}");
			if (beforeWeapon != null)
			{
				slot.AddItem(beforeWeapon);
			}
		}
	}

	private void DestroyCurrentEquipments()
	{
		foreach (Transform child in panelCharacter.transform)
		{
			if (child.gameObject.activeSelf)
			{
				Transform rightHand = child.FindDeepChild("Sword");

				for (int i = 0; i < rightHand.childCount; i++)
				{
					Destroy(rightHand.GetChild(i).gameObject);
				}
			}
		}
	}

	public void SetPanelSwordCharacter(ItemBase item)
	{
		foreach (Transform child in panelCharacter.transform)
		{
			if (child.gameObject.activeSelf)
			{
				Transform rightHand = child.FindDeepChild("Sword");

				GameObject panelSword = Instantiate(item.equipPrefab, rightHand);
			}
		}
	}


	private void UpdateUI()
	{
		if (currentEquipImage.sprite == null)
		{
			currentEquipImage.enabled = false;
		}

		else
		{
			currentEquipImage.enabled = true;
		}
	}
}