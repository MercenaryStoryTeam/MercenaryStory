using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TestSY : MonoBehaviour
{
    public ItemBase currentWeapon;
    public float myGold = 0;
    
    private PlayerTestSY playerTest;
    private bool isPlayerSpawned = false;
    
    
    private void Start()
    {
    }

    private void Update()
    {
        if(!isPlayerSpawned)
        {
            playerTest = FindObjectOfType<PlayerTestSY>();

            if(playerTest != null )
            {
                isPlayerSpawned = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            ItemBase randomItem = TryBossDropItem(InventoryManger.Instance.allItems);
            if (randomItem != null)
            {
                if (playerTest != null)
                {
                    playerTest.InvenSceneTestDrop(randomItem);
                }
                else
                {
                    Debug.Log("PlayerTestSY 스크립트 참조 X");
                }
            }
        }
    }

    private ItemBase TryBossDropItem(List<ItemBase> items)
    {
        ItemBase droppedItem = null;

        float dropChance = UnityEngine.Random.Range(0f, 1f);
        if (dropChance <= 0.5f)
        {
            foreach (ItemBase item in items)
            {
                float randomValue = UnityEngine.Random.Range(0f, 1f);
                if (randomValue <= item.dropPercent)
                {
                    Debug.Log($"{dropChance}의 확률로 아이템 획득!");

                    droppedItem = item;
                    Debug.Log($"아이템 {droppedItem}을 {randomValue}의 확률로 얻음!");

                    break;
                }
            }
        }

        else
        {
            Debug.Log($"{dropChance}의 확률로 아이템을 획득하지 못함");
        }

        return droppedItem;
    }

}
