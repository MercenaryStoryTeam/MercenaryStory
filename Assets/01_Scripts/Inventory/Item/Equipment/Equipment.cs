using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEditor;
using UnityEngine;

public class Equipment : MonoBehaviourPunCallbacks
{
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
            if (savedWeaponId != 0)
            {
                ItemBase savedWeapon = InventoryManger.Instance.allItems.Find(x => x.id == savedWeaponId);
                if (savedWeapon != null)
                {
                    Debug.Log($"{savedWeapon.name} 으로 장비 프리팹 설정 완료");
                    photonView.RPC("NetworkSetEquipment", RpcTarget.All, savedWeaponId);
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
            Debug.Log("인벤토리매니저 없음");
            return;
        }

        if (InventoryManger.Instance.allItems == null)
        {
            Debug.Log("인벤토리 매니저의 allItems가 비어있음");
            return;
        }
        
        ItemBase item = InventoryManger.Instance.allItems.Find(x => x.id == itemId);
        if (item == null)
        {
            return;
        }

        if (item != null)
        {
            FirebaseManager.Instance.CurrentUserData.user_weapon_item_Id = itemId;
            FirebaseManager.Instance.UploadCurrentUserData("user_weapon_item_Id", itemId);
            DestroyChildObject();
            SetSwordClass(item);
        }
    }

    private void SetSwordClass(ItemBase item)
    {
        GameObject playerPrefab = GameObject.Find($"{FirebaseManager.Instance.CurrentUserData.user_Name}");
        equipmentParent = playerPrefab.transform.FindDeepChild("Sword");
        
        if (item is WeaponItem weapon)
        {
                if (weapon.rank == 1) // 장비 랭크가 1인 경우
                { 
                    rarity = 1; 
                    equipmentName = "PT_Longsword_01_a"; 
                    currentSword = ServerManager.PlayerEquip(rarity, equipmentName, equipmentParent);
                }
                else if (weapon.rank == 2) 
                { 
                    rarity = 2; 
                    equipmentName = "PT_Longsword_04_a"; 
                    currentSword = ServerManager.PlayerEquip(rarity, equipmentName, equipmentParent); 
                }
                else if(weapon.rank == 3) 
                { 
                    rarity = 3; 
                    equipmentName = "PT_Longsword_03_a"; 
                    currentSword = ServerManager.PlayerEquip(rarity, equipmentName, equipmentParent); 
                }
                
                else if (weapon.rank == 4) 
                { 
                    rarity = 4; 
                    equipmentName = "PT_Longsword_02_c"; 
                    currentSword = ServerManager.PlayerEquip(rarity, equipmentName, equipmentParent); 
                }
                
                else if (weapon.rank == 5) 
                { 
                    rarity = 5; 
                    equipmentName = "PT_Longsword_01_c"; 
                    currentSword = ServerManager.PlayerEquip(rarity, equipmentName, equipmentParent); 
                } 
            
        }
    }

   
    
    private void DestroyChildObject()
    {
        GameObject playerPrefab = GameObject.Find($"{FirebaseManager.Instance.CurrentUserData.user_Name}");
        GameObject findSword = playerPrefab.transform.FindDeepChild("Sword").gameObject;
  
        
        for (int i = 0; i < findSword.transform.childCount; i++)
        { 
            Destroy(findSword.transform.GetChild(i).gameObject);
        }
        
    }
    

}
