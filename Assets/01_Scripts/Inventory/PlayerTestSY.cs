using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTestSY : MonoBehaviour
{
    private float interactRange = 3f;
    private MonsterTest monster;
    
    private List<(GameObject droppedLightLine, ItemBase droppedItem)> droppedItems = new List<(GameObject droppedLightLine, ItemBase droppedItem)>();
    private void Awake()
    {
        monster = FindObjectOfType<MonsterTest>();
    }

    private void Update()
    {
        if (droppedItems.Count > 0)
        {
            for (int i = droppedItems.Count - 1; i >= 0;  i--)
            {
                if (droppedItems[i].droppedItem == null || droppedItems[i].droppedLightLine == null)
                {
                    print("현재 드랍된 아이템 없음");
                }

                if (Vector3.Distance(transform.position, droppedItems[i].droppedLightLine.transform.position) <
                    interactRange)
                {
                    print("상호작용 거리임");
                    if (Input.GetKeyDown(KeyCode.E)) // E키 누르면 반경 안에 있는 아이템 인벤토리로 들어감. 테스트용 키임
                    {
                        InventoryManger.Instance.AddItemToInventory(droppedItems[i].droppedItem);
                        Destroy(droppedItems[i].droppedLightLine.gameObject);
                        droppedItems.RemoveAt(i);
                    }
                }
            }
        }

    }

    public void InvenSceneTestDrop(ItemBase item)
    {
        GameObject itemLightLine = Instantiate(item.dropLightLine, transform.position, Quaternion.identity);
        droppedItems.Add((itemLightLine, item));
    }

    public void TestDrop(ItemBase item)
    {
        GameObject itemLightLine = Instantiate(item.dropLightLine, monster.transform.position, Quaternion.identity);
        droppedItems.Add((itemLightLine, item));
    }
}
