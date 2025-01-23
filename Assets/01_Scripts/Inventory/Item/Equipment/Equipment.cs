 using Photon.Pun;
using UnityEngine;

public class Equipment : MonoBehaviourPunCallbacks
{
	public GameObject[] equipments;
	// Equipment Destroy 하기 위해 대입할 GameObject
	private GameObject currentSword;

	// 임시 값들. 상황에 맞게 수정해야함.
	private int rarity;
	private string equipmentName;

	// equipmentParent : rightHand, leftHand의 자식인 Shield 등을 Find해서 대입하는 로직 필요
	private Transform equipmentParent;

	private void Start()
	{
		if (photonView.IsMine)
		{
			int savedWeaponId = FirebaseManager.Instance.CurrentUserData.user_weapon_item_Id;
			Debug.Log($"Start - Firebase에서 불러온 무기 ID: {savedWeaponId}");
			if (savedWeaponId != 0)
			{
				ItemBase savedWeapon = InventoryManger.Instance.allItems.Find(x => x.id == savedWeaponId);
				if (savedWeapon != null)
				{
					Debug.Log($"{savedWeapon.itemName}({savedWeapon.id}) 으로 장비 프리팹 설정 시도");
					photonView.RPC("NetworkSetEquipment", RpcTarget.All, savedWeaponId);
					Debug.Log($"{savedWeapon.itemName}으로 장비 장착함.");
				}
			}
		}
	}

	[PunRPC]
	public void SetCurrentEquip(InventorySlot slot)
	{
		if (!photonView.IsMine) return;

		if (slot.item == null)
		{
			Debug.Log("아이템이 없음");
			return;
		}

		if (slot.item.equipPrefab == null)
		{
			Debug.Log("프리팹 리스트가 비어있거나 아이템이 할당되지 않음");
			return;
		}

		if (slot.item.equipPrefab != null)
		{
			photonView.RPC("NetworkSetEquipment", RpcTarget.All, slot.item.id);
		}
	}

	[PunRPC]
	private void NetworkSetEquipment(int itemId)
	{
		
		if (InventoryManger.Instance == null)
		{
			Debug.LogError("인벤토리매니저 없음");
			return;
		}

		if (InventoryManger.Instance.allItems == null)
		{
			Debug.LogError("인벤토리 매니저의 allItems가 비어있음");
			return;
		}

		ItemBase item = InventoryManger.Instance.allItems.Find(x => x.id == itemId);
		if (item == null)
		{
			Debug.LogError($"ID {itemId}에 해당하는 아이템을 찾을 수 없음");
			return;
		}

		
		if (photonView.IsMine)
		{
			FirebaseManager.Instance.CurrentUserData.user_weapon_item_Id = itemId;
			FirebaseManager.Instance.UploadCurrentUserData("user_weapon_item_Id", itemId);
		}
		SetSwordClass(item);
	}

	private void SetSwordClass(ItemBase item)
	{
		GameObject playerPrefab =
			GameObject.Find($"{FirebaseManager.Instance.CurrentUserData.user_Name}");
		equipmentParent = playerPrefab.transform.FindDeepChild("Sword");

		if (item is WeaponItem weapon)
		{
			if (weapon.rank == 1) // 장비 랭크가 1인 경우
			{
				equipments[0].SetActive(true);
				equipments[1].SetActive(false);
				equipments[2].SetActive(false);
				equipments[3].SetActive(false);
				equipments[4].SetActive(false);
			}
			else if (weapon.rank == 2)
			{
				equipments[0].SetActive(false);
				equipments[1].SetActive(true);
				equipments[2].SetActive(false);
				equipments[3].SetActive(false);
				equipments[4].SetActive(false);
			}
			else if (weapon.rank == 3)
			{
				equipments[0].SetActive(false);
				equipments[1].SetActive(false);
				equipments[2].SetActive(true);
				equipments[3].SetActive(false);
				equipments[4].SetActive(false);
			}

			else if (weapon.rank == 4)
			{
				equipments[0].SetActive(false);
				equipments[1].SetActive(false);
				equipments[2].SetActive(false);
				equipments[3].SetActive(true);
				equipments[4].SetActive(false);
			}

			else if (weapon.rank == 5)
			{
				equipments[0].SetActive(true);
				equipments[1].SetActive(false);
				equipments[2].SetActive(false);
				equipments[3].SetActive(false);
				equipments[4].SetActive(true);
			}
		}
	}
}