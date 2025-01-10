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

    public Button sellButton;
    public Button closeButton;
    
    private TestSY _testsy;
    private void Awake()
    {
        _testsy = FindObjectOfType<TestSY>();
        shopPanel.SetActive(false);
        ShopButtonClicked();
    }

    private void Update()
    {
        SetGold();
    }

    private void ShopButtonClicked()
    {
        sellButton.onClick.AddListener(SellButtonClick);
        closeButton.onClick.AddListener(CloseButtonClick);
    }

    private void CloseButtonClick()
    {
        UIManager.Instance.isShopActive = false;
        UIManager.Instance.CloseShopPanel();
    }

    private void SellButtonClick()
    {
        UIManager.Instance.CloseShopPanel();
        //아이템 삭제 로직
        //플레이어 골드 변경 로직
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
