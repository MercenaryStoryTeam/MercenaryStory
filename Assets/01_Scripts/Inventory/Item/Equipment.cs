using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    public GameObject rightHand;
    public GameObject panelRightHand;
    public GameObject leftHand;
    public GameObject panelLeftHand;
    
    private TestSY _testSY;

    private void Awake()
    {
        _testSY = FindObjectOfType<TestSY>();
    }
    
    public void SetCurrentEquip(InventorySlot slot)
    {
        if (_testSY.currentWeapon == null)
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
                Instantiate(slot.item.equipPrefab[0], rightHand.transform);
                GameObject panelSword = Instantiate(slot.item.equipPrefab[0], panelRightHand.transform);
                panelSword.layer = LayerMask.NameToLayer("Object");

                if (slot.item.equipPrefab.Count > 1)
                {
                    Instantiate(slot.item.equipPrefab[1], leftHand.transform);
                    GameObject panelShield = Instantiate(slot.item.equipPrefab[1], panelLeftHand.transform);
                    panelShield.layer = LayerMask.NameToLayer("Object");
                    for (int i = 0; i < panelShield.transform.childCount; i++)
                    {
                        panelShield.transform.GetChild(i).gameObject.layer = LayerMask.NameToLayer("Object");
                    }
                }
            
                Debug.Log($"{slot.item.name} 장착 완료");
            }

        }

        else
        {
            ItemBase beforeWeapon = _testSY.currentWeapon;
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
                DestroyChildObject();
                Instantiate(slot.item.equipPrefab[0], rightHand.transform);
                GameObject panelSword = Instantiate(slot.item.equipPrefab[0], panelRightHand.transform);
                panelSword.layer = LayerMask.NameToLayer("Object");
                
                if (slot.item.equipPrefab.Count > 1)
                {
                    Instantiate(slot.item.equipPrefab[1], leftHand.transform);
                    GameObject panelShield = Instantiate(slot.item.equipPrefab[1], panelLeftHand.transform);
                    panelShield.layer = LayerMask.NameToLayer("Object");
                    for (int i = 0; i < panelShield.transform.childCount; i++)
                    {
                        panelShield.transform.GetChild(i).gameObject.layer = LayerMask.NameToLayer("Object");
                    }
                }
                Debug.Log($"{slot.item.name} 장착 완료");
            }

        }
    }

    public void DestroyChildObject()
    {
        for (int i = 0; i < rightHand.transform.childCount; i++)
        {
            Destroy(rightHand.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < leftHand.transform.childCount; i++)
        {
            Destroy(leftHand.transform.GetChild(i).gameObject);
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
