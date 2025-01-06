using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TestSY : MonoBehaviour
{
    private InvenUI invenUI; // 테스트용. 나중에 UIManager로 변경 예정
    private int currentOption;
    
    private void Awake()
    {
        invenUI = FindObjectOfType<InvenUI>();
    }

    private void Update()
    {
        invenUI.TryOpenInventory();
    }

    private void ItemDropTest()
    {
        //ItemManager.Instance.AddItem();
        int randomDrop = Random.Range(0, 4);
        if (Input.GetKeyDown(KeyCode.O))
        {
            
        }
    }
}
