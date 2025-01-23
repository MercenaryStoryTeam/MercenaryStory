 using Photon.Pun;
using UnityEngine;

public class Equipment : MonoBehaviourPunCallbacks
{
	public GameObject[] equipments;

	private void Start()
	{
		if (photonView.IsMine)
		{
			int savedWeaponId = FirebaseManager.Instance.CurrentUserData.user_weapon_item_Id;
			if (savedWeaponId != 0)
			{
				ItemBase savedWeapon = InventoryManger.Instance.allItems.Find(x => x.id == savedWeaponId);
				if (savedWeapon != null)
				{
					photonView.RPC("NetworkSetEquipment", RpcTarget.All, savedWeaponId);
					Debug.Log($"{savedWeapon.itemName}으로 장비 장착함.");
				}
			}
		}
	}

	public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
	{
		// 포톤 네트워크 콜백 메서드
		base.OnPlayerEnteredRoom(newPlayer);

		if (photonView.IsMine)
		{
			int currentWeaponId = FirebaseManager.Instance.CurrentUserData.user_weapon_item_Id;
			if (currentWeaponId != 0)
			{
				photonView.RPC("NetworkSetEquipment", RpcTarget.All, currentWeaponId);
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
				equipments[0].SetActive(false);
				equipments[1].SetActive(false);
				equipments[2].SetActive(false);
				equipments[3].SetActive(false);
				equipments[4].SetActive(true);
			}
		}
	}
}