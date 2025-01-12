using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour
{
    public GameObject shopPanel;
    public Text sellPriceText;
    public Text currentGoldText;
    public List<InventorySlot> holdSlots;  // 보유 물품 슬롯들
    public List<InventorySlot> sellSlots;  // 판매할 물품 슬롯들

    public Button sellButton;
    public Button closeButton;
    
    private TestSY _testsy;
    private InventorySlot selectedSlot;
    private InventorySlot sellSlot;

    private void Awake()
    {
        _testsy = FindObjectOfType<TestSY>();
        shopPanel.SetActive(false);
        ShopButtonClicked();
    }

    private void Update()
    {
        SetGold();
        UpdateHoldSlots(); 
    }

    private void UpdateHoldSlots()
    {
        foreach (InventorySlot holdSlot in holdSlots)
        {
            holdSlot.RemoveItem();
        }

        List<InventorySlot> inventorySlots = UIManager.Instance.inventorySystem.slots;
        for (int i = 0; i < inventorySlots.Count && i < holdSlots.Count; i++)
        {
            if (inventorySlots[i].item != null)
            {
                holdSlots[i].AddItem(inventorySlots[i].item);
            }
        }
    }

    private void ShopButtonClicked()
    {
        closeButton.onClick.AddListener(CloseButtonClick);
    }

    private void CloseButtonClick()
    {
        foreach (InventorySlot slot in sellSlots)
        {
            slot.RemoveItem();
        }
        
        UIManager.Instance.isShopActive = false;
        UIManager.Instance.CloseShopPanel();
    }

    public void TryOpenShop()
    {
        if (Input.GetKeyDown(KeyCode.O)) // 테스트용 키 설정
        {
            if (!UIManager.Instance.isShopActive)
            {
                UIManager.Instance.OpenShopPanel();
            }
            else
            {
                foreach (InventorySlot slot in sellSlots)
                {
                    slot.RemoveItem();
                }

                UIManager.Instance.isShopActive = false;
                UIManager.Instance.CloseShopPanel();
            }
        }
    }

    private void SetGold()
    {
        currentGoldText.text = "보유 골드: " + _testsy.myGold.ToString();
        //판매용 골드 로직 추가
    }
}
