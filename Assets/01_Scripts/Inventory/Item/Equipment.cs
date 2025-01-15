using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    public GameObject rightHand;
    public GameObject leftHand;
    
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

            if (slot.item.prefab == null || slot.item.prefab.Count == 0)
            {
                Debug.Log("프리팹 리스트가 비어있거나 아이템이 할당되지 않음");
                return;
            }
        
            if (slot.item.prefab.Count > 0) 
            {
                Instantiate(slot.item.prefab[0], rightHand.transform);


                if (slot.item.prefab.Count > 1)
                {
                    Instantiate(slot.item.prefab[1], leftHand.transform);
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

            if (slot.item.prefab == null || slot.item.prefab.Count == 0)
            {
                Debug.Log("프리팹 리스트가 비어있거나 아이템이 할당되지 않음");
                return;
            }
        
            if (slot.item.prefab.Count > 0)
            {
                DestroyChildObject();
                Instantiate(slot.item.prefab[0], rightHand.transform);
                print($"오른손에 장착한 무기: {slot.item.prefab[0].name}");
                if (slot.item.prefab.Count > 1)
                {
                    Instantiate(slot.item.prefab[1], leftHand.transform);
                    print($"왼손에 장착한 무기: {slot.item.prefab[1].name}");
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
            print(rightHand.transform.GetChild(i).gameObject.name);
        }

        for (int i = 0; i < leftHand.transform.childCount; i++)
        {
            Destroy(leftHand.transform.GetChild(i).gameObject);
            print(leftHand.transform.GetChild(i).gameObject.name);
        }
    }
    

}
