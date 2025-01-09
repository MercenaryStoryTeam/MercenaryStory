using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour
{
    public GameObject shopPanel;
    public Text sellPriceText;
    public Text currentGoldText;

    public Button sellButton;
    public Button closeButton;

    private void Awake()
    {
        shopPanel.SetActive(false);
        ShopButtonClicked();
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
        if (Input.GetKeyDown(KeyCode.O))
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
}
