using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentPanel : MonoBehaviour
{
    public Image currentEquipImage;
    public GameObject firstCharacter;
    public GameObject secondCharacter;
    public GameObject thirdCharacter;
    public ItemBase currentItem; //현재 장착중인 아이템. 플레이어 스크립트에 넣어야할드 ㅅ
    
    private TestSY _testSY;
    
    private void Awake()
    {
        _testSY = FindObjectOfType<TestSY>();
        firstCharacter.SetActive(false);
        secondCharacter.SetActive(false);
        thirdCharacter.SetActive(false);
        
    }

    private void Start()
    {
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

        currentEquipImage.sprite = currentItem.image;
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
            int currentWeaponId = FirebaseManager.Instance.CurrentUserData.user_weapon_item_Id;
            currentWeaponId = slot.item.id;
            currentEquipImage.sprite = slot.item.image;
            
            slot.RemoveItem();
            print(beforeWeapon);
            print($"현재 장착한 아이템: {currentItem.name}, 장착한 아이탬 개수: {currentItem.currentItemCount}");
            if (beforeWeapon != null)
            {
                slot.AddItem(beforeWeapon);
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
