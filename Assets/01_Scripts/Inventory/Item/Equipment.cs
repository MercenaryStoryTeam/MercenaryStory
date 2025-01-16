using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    public GameObject panelRightHand;
    public GameObject panelLeftHand;
    public GameObject panelCharacter; // UI용 캐릭터 이미지를 가지고 있는 부모 오브젝트
    
    private TestSY _testSY;

    // Equipment Destroy 하기 위해 대입할 GameObject
    private GameObject currentSword;
    private GameObject currentShield;

    // 임시 값들. 상황에 맞게 수정해야함.
    private int rarity;
    private string equipmentName;

    // equipmentParent : rightHand, leftHand의 자식인 Shield 등을 Find해서 대입하는 로직 필요
    private Transform equipmentParent;
    
    
    private void Awake()
    {
        _testSY = FindObjectOfType<TestSY>();
    }
    
    public void SetCurrentEquip(InventorySlot slot)
    {
            if (slot.item == null)
            {
                Debug.Log("아이템이 없음");
                return;
            }

            if (slot.item.equipPrefab == null || slot.item.equipPrefab.Count == 0)
            {
                Debug.Log("프리팹 리스트가 비어있거나 아이템이 할당되지 않음");
                return;
            }
        
            if (slot.item.equipPrefab.Count > 0) 
            {
                SetSwordClass(slot.item);
                SetPanelSwordCharacter(slot.item);
                slot.item.isEquipped = true;

                if (slot.item.equipPrefab.Count > 1)
                {
                    SetShieldClass(slot.item);
                    SetPanelShieldCharacter(slot.item);
                }
            
                Debug.Log($"{slot.item.name} 장착 완료");
            }
        
    }

    private void SetPanelSwordCharacter(ItemBase item)
    {
        foreach (Transform child in panelCharacter.transform)
        {
            if (child.gameObject.activeSelf)
            {
                Transform rightHand = child.Find("Sword");

                GameObject panelSword = Instantiate(item.equipPrefab[0], rightHand);
                panelSword.layer = LayerMask.NameToLayer("Object");
            }
        }
    }

    private void SetPanelShieldCharacter(ItemBase item)
    {
        foreach (Transform child in panelCharacter.transform)
        {
            if (child.gameObject.activeSelf)
            {
                if (child.gameObject.activeSelf)
                {
                    Transform leftHand = child.Find("Shield");
                    
                    GameObject panelShield = Instantiate(item.equipPrefab[1], leftHand);
                    panelShield.layer = LayerMask.NameToLayer("Object");
                }
            }
        }
    }

    private void SetSwordClass(ItemBase item)
    {
        DestroyChildObject();
        GameObject playerPrefab = GameObject.Find($"{FirebaseManager.Instance.CurrentUserData.user_Name}");
        equipmentParent = playerPrefab.transform.Find("Sword");
        
        if (item is WeaponItem weapon)
        {
            if (item.equipPrefab.Count == 1) //아이템 프리팹 카운트가 1일 경우 -> 양손검일 경우
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
            
            else if (item.equipPrefab.Count == 2) // 아이템 프리팹 카운트가 2일 경우 -> 한손검 + 방패일 경우 
            {
                if (weapon.rank == 1)
                {
                    rarity = 1;
                    equipmentName = "PT_Sword_02_a";
                    currentSword =
                        ServerManager.PlayerEquip(rarity, equipmentName, equipmentParent);

                }
            
                else if (weapon.rank == 2)
                {
                    rarity = 2;
                    equipmentName = "PT_Sword_01_b";
                    currentSword = 
                        ServerManager.PlayerEquip(rarity, equipmentName, equipmentParent);
                }

                else if(weapon.rank == 3)
                {
                    rarity = 3;
                    equipmentName = "PT_Sword_05_a";
                    currentSword =
                        ServerManager.PlayerEquip(rarity, equipmentName, equipmentParent);
                }

                else if (weapon.rank == 4)
                {
                    rarity = 4;
                    equipmentName = "PT_Sword_05_b";
                    currentSword =
                        ServerManager.PlayerEquip(rarity, equipmentName, equipmentParent);
                }

                else if (weapon.rank == 5)
                {
                    rarity = 5;
                    equipmentName = "PT_Sword_01_c";
                    currentSword =
                        ServerManager.PlayerEquip(rarity, equipmentName, equipmentParent);
                }
            }
        }
    }

    private void SetShieldClass(ItemBase item)
    {
        GameObject playerPrefab = GameObject.Find($"{FirebaseManager.Instance.CurrentUserData.user_Name}");
        equipmentParent = playerPrefab.transform.Find("Shield");
        
        if (item is WeaponItem weapon)
        {
            if (weapon.rank == 1)
            {
                rarity = 1;
                equipmentName = "PT_Shield_01_a";
                currentSword =
                    ServerManager.PlayerEquip(rarity, equipmentName, equipmentParent);

            }
            
            else if (weapon.rank == 2)
            {
                rarity = 2;
                equipmentName = "PT_Shield_04_b";
                currentSword = 
                    ServerManager.PlayerEquip(rarity, equipmentName, equipmentParent);
            }

            else if(weapon.rank == 3)
            {
                rarity = 3;
                equipmentName = "PT_Shield_13_a";
                currentSword =
                    ServerManager.PlayerEquip(rarity, equipmentName, equipmentParent);
            }

            else if (weapon.rank == 4)
            {
                rarity = 4;
                equipmentName = "PT_Shield_12_b";
                currentSword =
                    ServerManager.PlayerEquip(rarity, equipmentName, equipmentParent);
            }

            else if (weapon.rank == 5)
            {
                rarity = 5;
                equipmentName = "PT_Shield_03_c";
                currentSword =
                    ServerManager.PlayerEquip(rarity, equipmentName, equipmentParent);
            }
        }
    }
    
    private void DestroyChildObject()
    {
        GameObject playerPrefab = GameObject.Find($"{FirebaseManager.Instance.CurrentUserData.user_Name}");
        GameObject findShield = playerPrefab.transform.Find("Shield").gameObject;
        GameObject findSword = playerPrefab.transform.Find("Sword").gameObject;
        
        for (int i = 0; i < findShield.transform.childCount; i++)
        {
            DestroyImmediate(findShield.transform.GetChild(i).gameObject);
        }
        
        for (int i = 0; i < findSword.transform.childCount; i++)
        { 
            DestroyImmediate(findSword.transform.GetChild(i).gameObject);
        }
            

        for (int i = 0; i < panelRightHand.transform.childCount; i++)
        {
            Destroy(panelRightHand.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < panelLeftHand.transform.childCount; i++)
        {
            Destroy(panelLeftHand.transform.GetChild(i).gameObject);
        }
    }
    

}
