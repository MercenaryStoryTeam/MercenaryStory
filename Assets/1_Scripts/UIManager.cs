using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class UIManager : SingletonManager<UIManager>
{
    public ItemInfoPanel itemInfo;
    public InventoryPanel inventory;
    public ShopPanel shop;
    [HideInInspector]public bool isInventoryActive = false;

    protected override void Awake()
    {
        base.Awake();
        itemInfo = FindObjectOfType<ItemInfoPanel>();
        inventory = FindObjectOfType<InventoryPanel>();
        shop = FindObjectOfType<ShopPanel>();
    }

    #region Inventory
    
    public void OpenInventoryPanel()
    {
        isInventoryActive = true;
        inventory.panel.SetActive(true);
    }

    public void CloseInventoryPanel()
    {
        isInventoryActive = false;
        inventory.panel.SetActive(false);
    }

    #endregion
    
    #region Shop

    public void OpenShopPanel()
    {
        shop.shopPanel.SetActive(true);
    }

    public void CloseShopPanel()
    {
        shop.shopPanel.SetActive(false);
    }
    
    #endregion
}
