using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTestSY : MonoBehaviour
{
    private float interactRange = 3f;

    private void Awake()
    {
    }

    public void TestDrop(ItemBase item)
    {
        
        //Instantiate -> 포지션 몬스터의 위치 포지션으로 변경 예정
        GameObject itemLightLine = Instantiate(item.dropLightLine, /*몬스터*/transform.position, Quaternion.identity);

        if(Vector3.Distance(transform.position, itemLightLine.transform.position) < interactRange)
        {
            print("상호작용 거리임");
            if (Input.GetKeyDown(KeyCode.L)) // L키 누르면 반경 안에 있는 아이템 인벤토리로 들어감. 테스트용 키임
            {
                InventoryManger.Instance.AddItemToInventory(item);
                Destroy(itemLightLine.gameObject);
            }
        }
    }
}
